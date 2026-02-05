Shader "Unlit/FeatureShader_SR"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Mask Sprite", 2D) = "white" {}

        _SpriteRect ("Sprite Rect", Vector) = (0,0,1,1)

        _SkinColor ("Skin Color", Color) = (1,1,1,1)
        _SelectedColor ("Selected Color", Color) = (1,1,1,1)
        _ClassColor ("Class Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;

            float4 _SpriteRect;

            fixed3 _SkinColor;
            fixed3 _SelectedColor;
            fixed3 _ClassColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 maskUV : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = v.uv;

                o.maskUV = _SpriteRect.xy + v.uv * _SpriteRect.zw;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 main = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.maskUV);

                mask.rgb *= mask.a;

                fixed3 result = main.rgb;

                result = lerp(result, result * _SkinColor.rgb, mask.r);
                result = lerp(result, result * _SelectedColor.rgb, mask.g);
                result = lerp(result, result * _ClassColor.rgb, mask.b);

                return fixed4(result, main.a);
            }
            ENDHLSL
        }
    }
}
