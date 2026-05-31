Shader "UI/RadialAfterimage"
{
    Properties
    {
        // 残像生成元となる RenderTexture
        _MainTex ("Texture", 2D) = "white" {}

        // 表示する残像数
        _SampleCount ("Sample Count", Range(1, 8)) = 6

        // 残像同士の拡大率間隔
        _ScaleStep ("Scale Step", Range(0.01, 1.0)) = 0.15

        // 残像が外側へ流れる速度
        _ScaleSpeed ("Scale Speed", Range(0.0, 10.0)) = 1.0

        // 透明度が0になる距離
        _AlphaFadeDistance ("Alpha Fade Distance", Range(0.01, 10.0)) = 1.0

        // エフェクト全体の強さ
        _EffectStrength ("Effect Strength", Range(0.0, 1.0)) = 1.0

        // クロマキー除去対象色
        _ChromaColor ("Chroma Color", Color) = (1,0,1,1)

        // クロマキー判定の許容範囲
        _Tolerance ("Tolerance", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha One

        Cull Off

        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)

            float _SampleCount;
            float _ScaleStep;
            float _ScaleSpeed;
            float _AlphaFadeDistance;
            float _EffectStrength;
            float4 _ChromaColor;
            float _Tolerance;

            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz);

                output.uv =
                    input.uv;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 result = 0;

                float totalWeight = 0.0;

                int sampleCount =
                    max(
                        (int)round(_SampleCount),
                        1);

                float wrappedOffset =
                    fmod(
                        _Time.y * _ScaleSpeed,
                        _ScaleStep);

                float inverseFadeDistance =
                    1.0 /
                    max(
                        _AlphaFadeDistance,
                        0.0001);

                // UV中心座標を事前計算
                float2 centeredUv =
                    input.uv -
                    0.5;

                // クロマキー判定用閾値を事前計算
                half toleranceSq =
                    (half)(_Tolerance * _Tolerance);

                half toleranceMax =
                    (half)(_Tolerance + 0.05);

                half toleranceMaxSq =
                    toleranceMax *
                    toleranceMax;

                // 初期スケール
                float scale =
                    1.0 +
                    wrappedOffset;

                [loop]
                for (int index = 0; index < sampleCount; index++)
                {
                    float2 sampleUv =
                        centeredUv /
                        scale +
                        0.5;

                    half4 color =
                        SAMPLE_TEXTURE2D(
                            _MainTex,
                            sampler_MainTex,
                            sampleUv);

                    // クロマキー色との差分
                    half3 delta =
                        color.rgb -
                        (half3)_ChromaColor.rgb;

                    // 二乗距離
                    half distanceSq =
                        dot(
                            delta,
                            delta);

                    // クロマキー判定
                    half chromaAlpha =
                        smoothstep(
                            toleranceSq,
                            toleranceMaxSq,
                            distanceSq);

                    // Scale=1.0からの距離を正規化
                    float normalizedDistance =
                        saturate(
                            (scale - 1.0) *
                            inverseFadeDistance);

                    // 距離減衰
                    float fade =
                        1.0 -
                        normalizedDistance;

                    // fade^4
                    float weight =
                        fade *
                        fade *
                        fade *
                        fade;

                    half finalAlpha =
                        chromaAlpha *
                        (half)weight;

                    // 重み付き加算
                    result.rgb +=
                        color.rgb *
                        finalAlpha;

                    totalWeight +=
                        weight;

                    // Alphaは最大値を保持
                    result.a =
                        max(
                            result.a,
                            finalAlpha);

                    // 次のサンプルへ
                    scale +=
                        _ScaleStep;
                }

                // 白飛び防止のため平均化
                result.rgb /=
                    max(
                        totalWeight,
                        0.0001);

                // エフェクト強度
                result.rgb *=
                    _EffectStrength;

                result.a *=
                    _EffectStrength;

                return result;
            }

            ENDHLSL
        }
    }
}