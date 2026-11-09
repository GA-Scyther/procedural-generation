// Stylised water for the Universal Render Pipeline (URP 12+ / Unity 2021.2+ / Unity 6).
// REQUIREMENT: enable "Depth Texture" on your URP Asset (or on the Camera),
// otherwise depth colour and shoreline foam will not work.
// Use a subdivided plane (not a quad) so the vertex waves have vertices to move.

Shader "Custom/Stylised Water (URP)"
{
    Properties
    {
        [Header(Colour)]
        _ShallowColor     ("Shallow Colour", Color) = (0.30, 0.85, 0.85, 0.55)
        _DeepColor        ("Deep Colour", Color) = (0.05, 0.20, 0.45, 0.95)
        _HorizonColor     ("Horizon / Fresnel Colour", Color) = (0.65, 0.85, 1.0, 1)
        _DepthMaxDistance ("Depth Fade Distance", Float) = 4

        [Header(Foam)]
        _FoamColor        ("Foam Colour (A = strength)", Color) = (1, 1, 1, 1)
        _FoamDistance     ("Shore Foam Distance", Float) = 0.6
        _FoamNoiseScale   ("Foam Noise Scale", Float) = 3
        _FoamSpeed        ("Foam Scroll Speed", Float) = 0.3

        [Header(Waves)]
        _WaveAmplitude    ("Wave Amplitude", Float) = 0.15
        _WaveLength       ("Wave Length", Float) = 8
        _WaveSpeed        ("Wave Speed", Float) = 1
        _WaveDirection    ("Wave Direction (XZ)", Vector) = (1, 0, 0.3, 0)

        [Header(Lighting)]
        _Smoothness       ("Smoothness", Range(0.01, 1)) = 0.85
        _SpecularStrength ("Specular Strength", Range(0, 2)) = 1
        _FresnelPower     ("Fresnel Power", Range(0.5, 8)) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "WaterForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float4 _HorizonColor;
                float  _DepthMaxDistance;
                float4 _FoamColor;
                float  _FoamDistance;
                float  _FoamNoiseScale;
                float  _FoamSpeed;
                float  _WaveAmplitude;
                float  _WaveLength;
                float  _WaveSpeed;
                float4 _WaveDirection;
                float  _Smoothness;
                float  _SpecularStrength;
                float  _FresnelPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                float  fogFactor  : TEXCOORD2;
            };

            // ---------------------------------------------------------------
            // Waves: two sine waves summed. Analytic derivatives give the normal,
            // so lighting is smooth even on a coarse mesh.
            // ---------------------------------------------------------------
            void SampleWaves(float2 xz, out float height, out float3 normal)
            {
                float2 d1 = normalize(_WaveDirection.xz + 1e-5);
                float2 d2 = normalize(d1 * 0.8 + float2(-d1.y, d1.x) * 0.6); // ~37 degrees off

                float t  = _Time.y * _WaveSpeed;
                float k1 = TWO_PI / max(_WaveLength, 0.01);
                float k2 = k1 * 1.7;
                float a1 = _WaveAmplitude;
                float a2 = _WaveAmplitude * 0.5;

                // Deep-water dispersion: longer waves travel faster.
                float p1 = k1 * dot(d1, xz) - t * sqrt(9.8 * k1);
                float p2 = k2 * dot(d2, xz) - t * sqrt(9.8 * k2);

                height = a1 * sin(p1) + a2 * sin(p2);

                float2 grad = a1 * k1 * cos(p1) * d1 + a2 * k2 * cos(p2) * d2; // (dh/dx, dh/dz)
                normal = normalize(float3(-grad.x, 1.0, -grad.y));
            }

            // ---------------------------------------------------------------
            // Value noise for breaking up the foam line.
            // ---------------------------------------------------------------
            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = Hash(i);
                float b = Hash(i + float2(1, 0));
                float c = Hash(i + float2(0, 1));
                float d = Hash(i + float2(1, 1));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);

                float h; float3 n;
                SampleWaves(positionWS.xz, h, n);
                positionWS.y += h;

                OUT.positionWS = positionWS;
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                OUT.fogFactor  = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- Water depth (distance from surface to what's behind it) ---
                float2 screenUV      = IN.screenPos.xy / IN.screenPos.w;
                float  sceneEyeDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float  waterEyeDepth = IN.screenPos.w;
                float  depthDiff     = max(sceneEyeDepth - waterEyeDepth, 0.0);
                float  depth01       = saturate(depthDiff / max(_DepthMaxDistance, 0.001));

                // --- Per-pixel normal from the waves ---
                float h; float3 N;
                SampleWaves(IN.positionWS.xz, h, N);

                float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                Light mainLight = GetMainLight();
                float3 L = mainLight.direction;
                float3 H = normalize(L + V);

                // --- Lighting ---
                float  wrapDiffuse = saturate(dot(N, L)) * 0.5 + 0.5;
                float  specPower   = exp2(10.0 * _Smoothness + 1.0);
                float  spec        = pow(saturate(dot(N, H)), specPower) * _SpecularStrength;
                float  fresnel     = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                float4 water = lerp(_ShallowColor, _DeepColor, depth01);

                float3 col = water.rgb * mainLight.color * wrapDiffuse;
                col += water.rgb * SampleSH(N) * 0.3;          // ambient
                col  = lerp(col, _HorizonColor.rgb, fresnel);   // sky reflection at grazing angles
                col += spec * mainLight.color;

                // --- Shoreline foam ---
                float noise    = ValueNoise(IN.positionWS.xz * _FoamNoiseScale + _Time.y * _FoamSpeed);
                float foamEdge = saturate(depthDiff / max(_FoamDistance, 0.001)); // 0 at the shore
                float foam     = smoothstep(foamEdge - 0.05, foamEdge + 0.05, noise) * _FoamColor.a;
                col = lerp(col, _FoamColor.rgb, foam);

                float alpha = saturate(max(water.a, fresnel) + foam + spec);

                col = MixFog(col, IN.fogFactor);
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
