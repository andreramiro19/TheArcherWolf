Shader "Archer Wolf/UI/Paper Overlay Multiply"
{
    Properties
    {
        _MainTex ("Paper Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Intensity ("Multiply Intensity", Range(0,1)) = 1
        _Contrast ("Darken Contrast", Range(0.1,4)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend DstColor Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Intensity;
            float _Contrast;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * i.color;
                // Push the texture's own darks further down before blending, since a
                // plain multiply against a light paper texture is very subtle on its own.
                fixed3 darkened = pow(saturate(tex.rgb), _Contrast);
                fixed3 multiplied = lerp(fixed3(1,1,1), darkened, _Intensity * tex.a);
                return fixed4(multiplied, 1);
            }
            ENDCG
        }
    }
}
