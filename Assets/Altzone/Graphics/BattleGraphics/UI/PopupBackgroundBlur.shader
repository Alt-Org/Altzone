Shader "UI/PopupBackgroundBlur"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 30)) = 8
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        GrabPass
        {
            "_PopupBlurGrab"
        }

        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _PopupBlurGrab;
            float4 _PopupBlurGrab_TexelSize;

            fixed4 _Color;
            float _BlurSize;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.color = v.color * _Color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.grabPos.xy / i.grabPos.w;

                float2 texel = _PopupBlurGrab_TexelSize.xy * _BlurSize;

                fixed4 col = 0;

                // Center
                col += tex2D(_PopupBlurGrab, uv) * 0.16;

                // Cardinal directions
                col += tex2D(_PopupBlurGrab, uv + float2(texel.x, 0)) * 0.10;
                col += tex2D(_PopupBlurGrab, uv + float2(-texel.x, 0)) * 0.10;
                col += tex2D(_PopupBlurGrab, uv + float2(0, texel.y)) * 0.10;
                col += tex2D(_PopupBlurGrab, uv + float2(0, -texel.y)) * 0.10;

                // Diagonals
                col += tex2D(_PopupBlurGrab, uv + float2(texel.x, texel.y)) * 0.085;
                col += tex2D(_PopupBlurGrab, uv + float2(-texel.x, texel.y)) * 0.085;
                col += tex2D(_PopupBlurGrab, uv + float2(texel.x, -texel.y)) * 0.085;
                col += tex2D(_PopupBlurGrab, uv + float2(-texel.x, -texel.y)) * 0.085;

                // Tint
                col.rgb *= i.color.rgb;

                // UI Image alpha
                col.a = i.color.a;

                return col;
            }
            ENDCG
        }
    }
}
