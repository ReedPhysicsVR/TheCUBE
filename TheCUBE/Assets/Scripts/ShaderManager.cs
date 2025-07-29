using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Communicates the required information to the heatmap shader
/// </summary>
public class ShaderManager : MonoBehaviour
{
    [Header("Screen Settings")]
    public Material screenMat;
    [HideInInspector]
    public Shader heatmapShader;
    public ColorRow[] palettes;
    private int palette_index;
    public InputActionReference swapPallette;

    [Header("Relevant Objects")]
    public GameObject sphere;
    private bool firstpass;
    private Transform sphereTransform;
    [HideInInspector]
    public PointMovement pointMovement;
    [SerializeField] private Slider maxScaleSlider;
    [SerializeField] private Slider solSlider;
    public GameObject speedLimitSign;

    //Shader property IDs
    private int posID, velID, accelID, ftID, fsID, solID, mndID;
    private LocalKeyword relKeyword, slowKeyword, fullKeyword, accelKeyword;
    public ShaderVariantCollection shaderVariants;

    [Header("Vector Field Settings")]
    public FieldType fieldType;
    private bool accelerationField;
    private bool relativistic;
    [HideInInspector]
    public float fieldScale;
    public bool magnitudeNotDot;
    public float scalingMinimum;
    [Tooltip("The max field strengths, X for magnitude, Y for dot, in order by field type enum")]
    public Vector2[] scalingMaximums;
    public float[] fieldOrderOfMag;
    [HideInInspector]
    public float speedOfLight;
    public Vector2 speedOfLightMinMax;
    public float speedOfLightScaleCurvature;

    private void Awake()
    {
        // Adds the SwapColorPalette function to the controller input action profile
        if (!swapPallette.action.enabled)
        {
            swapPallette.action.Enable();
        }
        swapPallette.action.performed += SwapColorPalette;
    }

    // Set initial values
    void Start()
    {
        firstpass = true;
        palette_index = 0;
        palettes = LinearizeRGB(palettes);

        // Sphere info init
        sphereTransform = sphere.GetComponent<Transform>();
        pointMovement = sphere.GetComponent<PointMovement>();

        magnitudeNotDot = false;

        //Slider scale init
        maxScaleSlider.minValue = scalingMinimum;
        maxScaleSlider.maxValue = scalingMaximums[(int)fieldType][Convert.ToInt16(magnitudeNotDot)];
        solSlider.minValue = -1f;
        solSlider.maxValue = 0f;
        speedOfLight = speedOfLightMinMax[1];
        relativistic = false;

        //Get Shader Property IDs
        posID = Shader.PropertyToID("_SpherePosition");
        velID = Shader.PropertyToID("_SphereVelocity");
        accelID = Shader.PropertyToID("_SphereAcceleration");
        fsID = Shader.PropertyToID("_FieldScale");
        ftID = Shader.PropertyToID("_FieldType");
        solID = Shader.PropertyToID("_SpeedOfLight");
        mndID = Shader.PropertyToID("_MagNotDot");

        //Set shader initial data
        screenMat.SetInteger(ftID, (int)fieldType);
        screenMat.SetInteger(mndID, Convert.ToInt16(magnitudeNotDot));
        screenMat.SetColorArray("_ColorArr", palettes[0].colors);
        screenMat.SetFloat(fsID, maxScaleSlider.value);
        screenMat.SetFloat(solID, speedOfLightMinMax[1]);
        screenMat.SetFloatArray("_OrderOfMag", fieldOrderOfMag);


        //Cache keywords, warm up variants, and set initial states
        foreach (var localKW in screenMat.shader.keywordSpace.keywords)
        {
            switch (localKW.name)
            {
                case "SLOW_SPEED":
                    slowKeyword = localKW;
                    break;
                case "RELATIVISTIC_SPEED":
                    relKeyword = localKW;
                    break;
                case "FULL_FIELD":
                    fullKeyword = localKW;
                    break;
                case "ACCELERATION_FIELD":
                    accelKeyword = localKW;
                    break;
            }
        }

        //Pre-load shaders if not already done
        if (!shaderVariants.isWarmedUp) shaderVariants.WarmUp();

        //Need to explicitly define the state of each keyword or unpredicable behavior will result
        screenMat.EnableKeyword(slowKeyword);
        screenMat.EnableKeyword(fullKeyword);
        screenMat.DisableKeyword(relKeyword);
        screenMat.DisableKeyword(accelKeyword);

    }

    void Update()
    {

        if (sphereTransform.hasChanged || firstpass)
        {
            screenMat.SetVector(posID, sphereTransform.position);
            screenMat.SetVector(velID, pointMovement.velocity);
            screenMat.SetVector(accelID, pointMovement.acceleration);
        }

        // display speed limit sign if point is moving faster than speed of light
        if (pointMovement.velocity.magnitude > speedOfLight && !speedLimitSign.activeSelf) speedLimitSign.SetActive(true);
        else if (pointMovement.velocity.magnitude < speedOfLight && speedLimitSign.activeSelf) speedLimitSign.SetActive(false);

        if (firstpass) firstpass = false;
    }

    private void OnDestroy()
    {
        swapPallette.action.Disable();
        swapPallette.action.performed -= SwapColorPalette;
    }

    /// <summary>
    /// Takes list of color palettes in the form of ColorRow and converts from the sRGB color space to the linear RGB color space.
    /// For some reason, Unity and shaders interpret color vectors in different color spaces, so this ensures that the colors entered in the inspector
    /// match the colors displayed by the shader.
    /// </summary>
    /// <param name="p">List of all color palettes from Unity inspector.</param>
    /// <returns>Color palettes in linear RGB color space to send to the shader</returns>
    public ColorRow[] LinearizeRGB(ColorRow[] p)
    {
        ColorRow[] o = p;
        for (int i = 0; i < o.Length; i++)
        {
            for (int j = 0; j < o[i].colors.Length; j++)
            {
                o[i].colors[j] = o[i].colors[j].linear;
            }
        }
        return o;
    }

    #region UIFunctions

    /// <summary>
    /// Function to be used by a UI button. Sets the field to display the magnitude of the field or the dot product with the display surface
    /// </summary>
    /// <param name="magnotdot">Boolean, true = mag, false = dot</param>
    public void SetMagOrDot(bool magnotdot)
    {
        magnitudeNotDot = magnotdot;
        screenMat.SetInteger(mndID, Convert.ToInt16(magnotdot));
        maxScaleSlider.maxValue = scalingMaximums[accelerationField ? (int)fieldType + 4 : (int)fieldType][Convert.ToInt16(magnotdot)];
    }

    /// <summary>
    /// Function to be used by a UI button. Sets the field type to display.
    /// </summary>
    /// <param name="i">0=E, 1=B, 2=S</param>
    public void SetFieldType(int i)
    {
        fieldType = (FieldType)i;
        SetFieldScale(scalingMinimum);
        maxScaleSlider.maxValue = scalingMaximums[accelerationField ? i + 4 : i][Convert.ToInt16(magnitudeNotDot)];
        maxScaleSlider.value = maxScaleSlider.minValue;
        screenMat.SetInteger(ftID, i);
    }

    /// <summary>
    /// Function to be used by a UI button. Toggles the display of the acceleration field vs full/velocity field
    /// </summary>
    public void ToggleAccelerationField()
    {
        accelerationField = !accelerationField;
        SetFieldScale(scalingMinimum);
        maxScaleSlider.maxValue = scalingMaximums[accelerationField ? (int)fieldType + 4 : (int)fieldType][Convert.ToInt16(magnitudeNotDot)];
        maxScaleSlider.value = maxScaleSlider.minValue;
        screenMat.SetKeyword(accelKeyword, accelerationField);
        screenMat.SetKeyword(fullKeyword, !accelerationField);
    }

    /// <summary>
    /// Function to be used by a UI slider. Sets the scaling of the displayed field.
    /// </summary>
    /// <param name="maxScale"></param>
    public void SetFieldScale(System.Single maxScale)
    // System.Single is the same as a float. It's written this way to allow the UI slider to use it.
    {
        fieldScale = maxScale;
        screenMat.SetFloat(fsID, maxScale);
    }

    /// <summary>
    /// Function to be used by a UI slider. Sets the speed of light and toggles the relativistic/slow assumption equations as needed.
    /// The function sets the speed of light along an exponential curve since the most interesting effects appear at the slowest speeds.
    /// </summary>
    /// <param name="sol">Must be between -1 and 0.</param>
    public void SetSpeedOfLight(System.Single sol)
    // System.Single is the same as a float. It's written this way to allow the UI slider to use it.
    {
        speedOfLight = Mathf.Pow(speedOfLightScaleCurvature, sol) * (speedOfLightMinMax[1] - speedOfLightMinMax[0]) + speedOfLightMinMax[0];
        screenMat.SetFloat(solID, speedOfLight);
        if (relativistic == (sol == 0))
        {
            relativistic = !relativistic;
            screenMat.SetKeyword(slowKeyword, !relativistic);
            screenMat.SetKeyword(relKeyword, relativistic);
        }
    }

    #endregion

    #region ControllerButtonFunctions

    /// <summary>
    /// Function to be used as part of an input action. Allows the user to press a button and switch between color palettes.
    /// </summary>
    /// <param name="context">Required for use with input actions.</param>
    public void SwapColorPalette(InputAction.CallbackContext context)
    // function for controller button (left secondary), swaps color palette
    {
        ++palette_index;
        if (palette_index >= palettes.Length) palette_index = 0;
        screenMat.SetColorArray("_ColorArr", palettes[palette_index].colors);
    }

    #endregion
}


