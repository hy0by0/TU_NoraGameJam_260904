// Sprite の透明度から外周を検出し、元絵の外側だけに白い線を描きます。
Shader "Nora/Sprites/Enemy White Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Sprite Tint", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width (source pixels)", Range(0, 24)) = 12
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 original = tex2D(_MainTex, input.uv);
                float nearbyAlpha = 0;

                // 内外２周×８方向を調べ、斜め方向も画像の輪郭に沿わせます。
                [unroll]
                for (int ring = 1; ring <= 2; ring++)
                {
                    float2 offset = _MainTex_TexelSize.xy * (_OutlineWidth * ring * 0.5);
                    nearbyAlpha = max(nearbyAlpha, tex2D(_MainTex, input.uv + float2( offset.x, 0)).a);
                    nearbyAlpha = max(nearbyAlpha, tex2D(_MainTex, input.uv + float2(-offset.x, 0)).a);
                    nearbyAlpha = max(nearbyAlpha, tex2D(_MainTex, input.uv + float2(0,  offset.y)).a);
                    nearbyAlpha = max(nearbyAlpha, tex2D(_MainTex, input.uv + float2(0, -offset.y)).a);
                    nearbyAlpha = max(nearbyAlpha, tex2D(_MainTex, input.uv + float2( offset.x,  offset.y)).a);
                    nearbyAlpha = max(nearbyAlpha, tex2D(_MainTex, input.uv + float2(-offset.x,  offset.y)).a);
                    nearbyAlpha = max(nearbyAlpha, tex2D(_MainTex, input.uv + float2( offset.x, -offset.y)).a);
                    nearbyAlpha = max(nearbyAlpha, tex2D(_MainTex, input.uv + float2(-offset.x, -offset.y)).a);
                }

                // 元画像の半透明なにじみを外側まで引き伸ばさず、明確な境界にします。
                float imageAlpha = original.a * input.color.a;
                float outlineAlpha = step(0.001, _OutlineWidth) * step(0.5, nearbyAlpha)
                    * (1 - original.a)
                    * _OutlineColor.a * input.color.a;
                float resultAlpha = imageAlpha + outlineAlpha;
                float3 imageRgb = original.rgb * input.color.rgb;
                float3 resultRgb = (imageRgb * imageAlpha + _OutlineColor.rgb * outlineAlpha)
                    / max(resultAlpha, 0.0001);
                return fixed4(resultRgb, resultAlpha);
            }
            ENDCG
        }
    }
}
