Shader "Unlit/HairShader"
{
    Properties
    {
        _MainTex ("Hair Sprite", 2D) = "white" {}
        _MaskTex ("Mask Sprite", 2D) = "white" {}
        _BodyTex ("Body Sprite", 2D) = "white" {}

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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            sampler2D _BodyTex;

            fixed4 _SkinColor;
            fixed4 _SelectedColor;
            fixed4 _ClassColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 hair = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.uv);
                fixed4 body = tex2D(_BodyTex, i.uv);

                mask.rgb *= mask.a;

                fixed3 hairColored = hair.rgb;
                hairColored = lerp(hairColored, hairColored * _SkinColor.rgb, mask.r);
                hairColored = lerp(hairColored, hairColored * _SelectedColor.rgb, mask.g);
                hairColored = lerp(hairColored, hairColored * _ClassColor.rgb, mask.b);

                float hairAlpha = hair.a * (1.0 - body.a);

                return fixed4(hairColored, hairAlpha);
            }
            ENDHLSL
        }
    }
}
