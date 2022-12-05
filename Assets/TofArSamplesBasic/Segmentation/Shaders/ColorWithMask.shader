﻿/*
 * SPDX-License-Identifier: (Apache-2.0 OR GPL-2.0-only)
 *
 * Copyright 2022 Sony Semiconductor Solutions Corporation.
 *
 */

Shader "TofAr/Segmentation/ColorWithMask" {
    Properties{
        _MainColor("Base", Color) = (0,0,0,0)
        _MaskTexHuman("Mask Human", 2D) = "black" {}
        _MaskTexSky("Mask Sky", 2D) = "black" {}
    [MaterialToggle]_useHuman("use Human", int) = 0
        [MaterialToggle]_invertHuman("invert Human", int) = 0
    [MaterialToggle]_useSky("use Sky", int) = 0
        [MaterialToggle]_invertSky("invert Sky", int) = 0
        [HideInInspector]_OffsetU("OffsetU", Range(-1.0,1.0)) = 0.0
        [HideInInspector]_OffsetV("OffsetV", Range(-1.0,1.0)) = 0.0
        [HideInInspector]_ScaleV("ScaleV", Range(0.0,2.0)) = 1.0
        [HideInInspector]_ScaleU("ScaleU", Range(0.0,2.0)) = 1.0
    }
        SubShader{

        Tags { "RenderType" = "Transparent"  "Queue" = "Transparent"}
        Cull Off ZWrite Off ZTest Always

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MaskTexHuman;
            sampler2D _MaskTexSky;
            float4 _MainColor;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv2 : TEXCOORD0;
                float2 uv3 : TEXCOORD1;
            };

            float4 _MaskTexHuman_ST;
            float4 _MaskTexSky_ST;

        float _useHuman;
            float _invertHuman;
        float _useSky;
            float _invertSky;

            float _ScaleU;
            float _ScaleV;
            float _OffsetU;
            float _OffsetV;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float2 texT = v.texcoord;
                texT.x = texT.x * _ScaleU + _OffsetU;
                texT.y = texT.y * _ScaleV + _OffsetV;

                o.uv2 = TRANSFORM_TEX(texT, _MaskTexHuman);
                o.uv3 = TRANSFORM_TEX(texT, _MaskTexSky);
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                half4 base = _MainColor;
                half4 maskHuman = tex2D(_MaskTexHuman, i.uv2);
                half4 maskSky = tex2D(_MaskTexSky, i.uv3);

                if (i.uv2.y <= 0 && i.uv2.y >= -1 && i.uv2.x >= 0 && i.uv2.x <= +1) {
                    maskHuman.x = maskHuman.x*_useHuman + (1.0f - maskHuman.x)*_invertHuman;
                } else {
                    maskHuman.x = _invertHuman;
                }
            
                if (i.uv3.y <= 0 && i.uv3.y >= -1 && i.uv3.x >= 0 && i.uv3.x <= +1) {
                    maskSky.x = maskSky.x*_useSky + (1.0f - maskSky.x)*_invertSky;
                } else {
                    maskSky.x = _useSky;
                }
            
                maskHuman.x = maskHuman.x > maskSky.x ? maskHuman.x : maskSky.x;
                base.w = maskHuman.x * maskHuman.x * maskHuman.x;

                base.w = base.w * _MainColor.w;

                return base;
            }
            ENDCG
            }
    }
        FallBack Off
}