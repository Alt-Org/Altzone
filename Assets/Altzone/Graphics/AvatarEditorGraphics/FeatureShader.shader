Shader "Unlit/FeatureShader"
{
    Properties
    {
        _MainTex ("Main Sprite", 2D) = "white" {}
        _SwapTex ("Palette Texture", 2D) = "white" {}
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
            sampler2D _SwapTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 src = tex2D(_MainTex, i.uv);
                

                fixed4 swap = tex2D(_SwapTex, float2(src.r, 0.5));
                src.r = src.g;

                fixed3 tinted = src.rgb * swap.rgb;


                fixed3 finalRGB = lerp(src.rgb, tinted, swap.a);

                return fixed4(finalRGB, src.a);
            }
            ENDHLSL
        }
    }
}
