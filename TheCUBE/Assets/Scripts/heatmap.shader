Shader "Unlit/heatmap"
{
    Properties
    {
        // These are the shader's parameters that we will alter from outside the shader, either by a script or in the inspector.
        _SpherePosition ("Sphere Position", Vector)= (1,1,1,1)
        _SphereVelocity ("Sphere Velocity", Vector)= (0,0,0)
        _SphereAcceleration ("Sphere Acceleration", Vector) = (0.0,0.0,0.0)
        _FieldType ("Field Type", Integer) = 0
        _MagNotDot ("Magnitude not dot product", Integer) = 0
        _MaxDistance ("Top Distance", float) = 0.0
        _FieldScale ("Field Scaler", float ) = 1.0
        _SpeedOfLight ("Speed of light", float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            // these lines define what functions are to be used as vertex and fragment shaders
            #pragma vertex vert
            #pragma fragment frag
            // defining shader keywords
            #pragma multi_compile SLOW_SPEED RELATIVISTIC_SPEED
            #pragma multi_compile FULL_FIELD ACCELERATION_FIELD

            // allows access to Unity-related functions/information
            #include "UnityCG.cginc"

            // This is the vertex information we are requesting from Unity, localName : HLSL_OUTPUT_NAME
            // 
            struct appdata {
                float4 vertex : POSITION; 
                float3 worldPos : TEXCOORD0;
                float3 normal : NORMAL;

                // This line is needed to render the shader in stereo (otherwise only renders in the left eye, very barfy)
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // This is the output information from the vertex shader and the info to be passed to the frag shader, localName : HLSL_INPUT_NAME
            struct v2f {
                float3 worldPos : TEXCOORD0;
                float4 pos : SV_POSITION;
                half3 worldNorm : TEXCOORD1;

                // Needed for rendering in stereo
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // vertex shader
            v2f vert (appdata v)
            {
                v2f o;
                
                // These three lines are needed for rendering in stereo
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                // The screen position, world position, and surface normal respectively
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldNorm = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            // Defining the properties within the actual program, but the values will be either set in the inspector, 
            // by ShaderManager, or as the default values defined above
            float _MaxDistance;
            fixed4 _SpherePosition;
            float3 _SphereVelocity;
            float3 _SphereAcceleration;
            int _FieldType;
            int _MagNotDot;
            float _SpeedOfLight;
            float _FieldScale;
            fixed4 _ColorArr[7];
            float _OrderOfMag[7];

            fixed4 frag (v2f i) : SV_Target
            {
                float3 curlyR = i.worldPos.xyz - _SpherePosition.xyz;
                float3 curlyRhat = normalize(curlyR);
                float curlyRmag = 1.0 / pow(length(curlyR),3);
                float3 vec = float3(0,0,0);
                
                #if SLOW_SPEED
                    #if FULL_FIELD
                        switch (_FieldType)
                        {
                            case 0:
                                vec = curlyRmag * curlyR ;
                                break;
                            case 1:
                                vec = -curlyRmag * cross(_SphereVelocity, curlyR);
                                break;
                            case 2:
                                vec = curlyRmag * curlyRmag * (pow(length(curlyR), 2) * _SphereVelocity - dot(curlyR, _SphereVelocity) * curlyR);
                                break;
                            default:
                                vec = curlyRmag * curlyR ;
                                break;
                        }

                    #endif
                    #if ACCELERATION_FIELD 
                        float3 Eacc = (curlyRhat * dot(curlyRhat, _SphereAcceleration) - _SphereAcceleration) / length(curlyR);
                        switch (_FieldType)
                        {
                            case 0:
                                vec = Eacc;
                                break;
                            case 1:
                                vec = cross(curlyRhat, Eacc);
                                break;
                            case 2:
                                vec = cross(Eacc, cross(curlyRhat, Eacc));
                                break;
                        }
                    #endif
                #endif
                // The seemingly unneccesary speed of light scalars are there to create a smooth transition between slow-limit and relativistic fields
                #if RELATIVISTIC_SPEED
                    float3 u = _SpeedOfLight * curlyRhat - _SphereVelocity;
                    float udotr = 1.0 / dot(curlyR, u);

                    #if FULL_FIELD
                        float3 relE = (_SpeedOfLight * _SpeedOfLight - length(_SphereVelocity) * length(_SphereVelocity)) * u;
                        relE *= length(curlyR) * udotr * udotr * udotr;
                        
                        switch (_FieldType)
                        {
                            case 0:
                                vec = relE;
                                break;
                            case 1:
                                vec = -_SpeedOfLight * cross(curlyRhat, relE);
                                break;
                            case 2:
                                vec = -_SpeedOfLight * cross(relE, cross(curlyRhat, relE));
                                break;
                        }

                    #endif
                    #if ACCELERATION_FIELD
                        float3 relE = _SpeedOfLight * _SpeedOfLight * cross(curlyR, cross(u, _SphereAcceleration));
                        relE *= length(curlyR) * udotr * udotr * udotr;
                        
                        switch (_FieldType)
                        {
                            case 0:
                                vec = relE;
                                break;
                            case 1:
                                vec = cross(curlyRhat, relE);
                                break;                        
                            case 2:
                                vec = cross(relE, cross(curlyRhat, relE));
                                break;
                        }
                    #endif
                #endif

                #if FULL_FIELD
                    vec *= _OrderOfMag [_FieldType];
                #endif
                #if ACCELERATION_FIELD
                    vec *= _OrderOfMag [_FieldType + 4];
                #endif

                float mag;
                // If displaying flux, shift domain from [-1, 1] to [0, 6].
                // If displaying magnitude, shift from [0, 1] to [0, 6].
                // So a color can be chosen from the 7 possible.
                if (_MagNotDot == 0)
                {
                    mag = 3 * clamp(dot(-vec, i.worldNorm) * _FieldScale, -1, 1) + 3;
                } else {
                    mag = 6 * clamp(length(vec) * _FieldScale, 0, 1);
                }
                
                //Ensure that the low end of the linear interpolation is not the highest-indexed color.
                int level = (int)floor(clamp(mag, 0, 5));
                return lerp(_ColorArr[level], _ColorArr[level+1], mag - level);
            }
            ENDCG
        }
    }
}
