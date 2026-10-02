Shader "Ducks/Player Rubber" {
 Properties { _Color("Player Color",Color)=(1,0.85,0.05,1) _MainTex("Original Texture",2D)="white" {} }
 SubShader { Tags { "RenderType"="Opaque" }
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex;
 fixed4 _Color;
 struct Input { float2 uv_MainTex; };
 void surf(Input IN,inout SurfaceOutputStandard o) {
 fixed3 c=tex2D(_MainTex,IN.uv_MainTex).rgb;
 float body=step(0.125,IN.uv_MainTex.x)*(1-step(0.25,IN.uv_MainTex.x));
 o.Albedo=lerp(c,_Color.rgb*lerp(0.75,1.0,max(c.r,c.g)),body);
 o.Metallic=0; o.Smoothness=0.32; o.Alpha=1;
 }
 ENDCG
 } FallBack "Diffuse"
}
