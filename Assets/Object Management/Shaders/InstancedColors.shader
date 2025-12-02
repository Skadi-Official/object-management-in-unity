Shader "Custom/InstancedColors" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)          // 基础颜色属性（会被实例化）
        _MainTex ("Albedo (RGB)", 2D) = "white" {}   // 主贴图
        _Glossiness ("Smoothness", Range(0,1)) = 0.5 // 光滑度
        _Metallic ("Metallic", Range(0,1)) = 0.0     // 金属度
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma instancing_options assumeuniformscaling // 启用 GPU Instancing，并假设实例缩放一致，提高效率
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;

        // ===============================
        // GPU Instancing 的核心部分
        // ===============================
        UNITY_INSTANCING_BUFFER_START(Props)  
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color) 
            // 声明每个实例独立的 _Color 属性
            // GPU 会为每个实例保存一个独立的颜色值
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // 读取实例化属性 _Color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) *
                      UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            o.Albedo = c.rgb;        // 设置颜色
            o.Metallic = _Metallic;  // 金属度
            o.Smoothness = _Glossiness; // 光滑度
            o.Alpha = c.a;            // 透明度
        }
        ENDCG
    }
    FallBack "Diffuse"
}
