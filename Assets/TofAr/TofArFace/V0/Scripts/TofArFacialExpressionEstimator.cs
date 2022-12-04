/*
 * Copyright 2018,2019,2020,2021,2022 Sony Semiconductor Solutions Corporation.
 *
 * This is UNPUBLISHED PROPRIETARY SOURCE CODE of Sony Semiconductor
 * Solutions Corporation.
 * No part of this file may be copied, modified, sold, and distributed in any
 * form or by any means without prior explicit permission in writing from
 * Sony Semiconductor Solutions Corporation.
 *
 */
using System;
using System.IO;
using TensorFlowLite.Runtime;
using UnityEngine;

namespace TofAr.V0.Face
{
    public enum FacialExpression
    {
        Japanese_A,
        Japanese_I,
        Japanese_U,
        Japanese_E,
        Japanese_O,
        EyeOpen,
        BrowUp
    }

    public class TofArFacialExpressionEstimator : Singleton<TofArFacialExpressionEstimator>
    {
        /// <summary>
        /// 表情推定結果をコールバック
        /// </summary>
        /// <param name="result">ジェスチャー認識の結果</param>
        public delegate void FacialExpressionEstimatedEventHandler(float[] result);

        /// <summary>
        /// 表情推定結果通知
        /// </summary>  
        public static event FacialExpressionEstimatedEventHandler OnFacialExpressionEstimated;

        /// <summary>
        /// aiueo認識で使用するTFLiteのExecMode
        /// </summary>        
        public TFLiteRuntime.ExecMode ExecMode
        {
            get
            {
                return execMode;
            }

            set
            {
                this.execMode = value;

                if (this.isEstimating)
                {
                    lock (this)
                    {
                        Init();
                    }
                }
            }
        }

        private TFLiteRuntime.ExecMode execMode = TFLiteRuntime.ExecMode.EXEC_MODE_CPU;

        /// <summary>
        /// aiueo認識で使用するスレッド数
        /// </summary>        
        public int ThreadsNum
        {
            get
            {
                return this.threadsNum;
            }
            set
            {
                this.threadsNum = value;

                if (this.isEstimating)
                {
                    lock (this)
                    {
                        Init();
                    }
                }
            }
        }

        private int threadsNum = 1;

        private TFLiteRuntime vowelDetector;
        private string networkName = "vowels.tflite";
        // DNN input buffer (allocated in Runtime side)
        private float[] input;

        private const int facialExpressionsCount = 7;
        // result 
        private float[] facialExpressionsResult = new float[facialExpressionsCount];

        // internal variables
        private float openedEye = 0.009f;
        private float closedEye = 0.006f;

        // internal variables
        private float upBrow = 0.0095f;
        private float downBrow = 0.009f;

        private bool isEstimating = false;

        public bool autoStart = true;

        /// <summary>
        ///  The index of the face we are detecting expressions for
        /// </summary>
        public int FacialExpressionDetectionFaceIndex { get; set; } = 0;

        private ulong lastFaceTimestamp;

        private float[] coeffs_a = new float[52] { 0f, 0f, 0.716984335f, 0.045410029f, 0.134937582f, 0.122535494f, 0f, 0f, 0.023791297f, 0.431265206f, 0f, 0.1156161f, 0.160550634f, 0f, 0f, 0.724687291f, 0f, 0.020593494f, 0.141638727f, 0.811743835f, 0f, 0.172078241f, 0.024293945f, 0.168941946f, 0f, 0.007416498f, 0f, 0f, 0f, 0.010384178f, 0.468596056f, 2.06893E-05f, 0.068599427f, 0.069868848f, 0.156436477f, 0f, 0f, 0.285347993f, 0f, 0.115082638f, 0.034222891f, 0.126840573f, 0f, 0.063606792f, 3.86346E-05f, 0f, 0f, 0.288194059f, 0.068342676f, 0f, 0f, 0f };
        private float[] coeffs_i = new float[52] { 0f, 0f, 0.659665259f, 0.020108827f, 0.421334228f, 0.297926192f, 0f, 0f, 0.123297209f, 0.238376694f, 0f, 0.037989803f, 0.412464655f, 0f, 0f, 0.65921616f, 0f, 0.061795684f, 0.438117302f, 0.125754547f, 0f, 0.219568284f, 0.136323753f, 0.408254609f, 0.001771017f, 0.031802607f, 0f, 0f, 0f, 0.014505993f, 0.253996476f, 0.000250742f, 0.042541555f, 0.38816243f, 0.338737572f, 0f, 0f, 0.98785089f, 0f, 0.008080422f, 0.016045604f, 0.307503717f, 0f, 0.010746919f, 0f, 0f, 0f, 0.972329437f, 0.413366192f, 0f, 0f, 0f };
        private float[] coeffs_u = new float[52] { 0f, 0f, 0.155650435f, 0.025036558f, 0.063078221f, 0.054428662f, 0f, 0f, 0.057574672f, 0.170967738f, 0f, 0.525191756f, 0.184090836f, 0f, 0f, 0.166240329f, 0f, 0.003747651f, 0.070433359f, 0.144776355f, 0f, 0.035672136f, 0.064045433f, 0.18283445f, 0f, 0.072420456f, 0f, 0f, 0.013850078f, 0.07108368f, 0.178563725f, 7.98117E-05f, 0.05952798f, 0.112394751f, 0.190033031f, 0f, 0f, 0.044936792f, 0f, 0.143451385f, 0.026298354f, 0.058113106f, 0.016454091f, 0.524185252f, 0.007798679f, 0f, 0f, 0.032807372f, 0.103850152f, 0f, 0f, 0f };
        private float[] coeffs_e = new float[52] { 0f, 0f, 0.590644492f, 0.088076307f, 0.266664269f, 0.231685039f, 0f, 0f, 0.067174385f, 0.467079021f, 0f, 0.354507761f, 0.385064009f, 0f, 0f, 0.604381795f, 0f, 0f, 0.332729522f, 0.117613971f, 0f, 0.310808624f, 0.052554938f, 0.515842784f, 0.024084369f, 0.039622373f, 0f, 0f, 0f, 0.037032875f, 0.456966785f, 0.000534625f, 0.046963859f, 0.627019484f, 0.468208555f, 0f, 0f, 0.502760267f, 0f, 0.086952542f, 0.003171539f, 0.280777853f, 0f, 0.053495587f, 0.030498652f, 0f, 0f, 0.481026234f, 0.550279917f, 0f, 0f, 0f };
        private float[] coeffs_o = new float[52] { 0f, 0f, 0.331421792f, 0.201694139f, 0.087454983f, 0.074916079f, 0f, 0f, 0.012075249f, 0.417560035f, 0f, 0.592829657f, 0.08127094f, 0f, 0f, 0.325128609f, 0f, 0f, 0.105545316f, 0.672644565f, 0f, 0.120194291f, 0.009800821f, 0.109707901f, 0.029621738f, 0.017056767f, 0f, 0f, 0.209186312f, 0.057833044f, 0.416595213f, 0.007259792f, 0.110363696f, 0.100958042f, 0.20470028f, 0f, 0f, 0f, 0f, 0.59909849f, 0f, 0.103092384f, 0.24022439f, 0.443989135f, 0.029227848f, 0f, 0f, 0f, 0.085702503f, 0f, 0f, 0f };

        private void Start()
        {
            if (autoStart)
            {
                StartGestureEstimation();
            }
        }

        /// <summary>
        /// ジェスチャー推定を開始する
        /// </summary>  
        public void StartGestureEstimation()
        {
            this.lastFaceTimestamp = 0;
            this.isEstimating = Init();
            TofArFaceManager.OnFaceEstimated += TofArFaceManager_OnFaceEstimated;
        }



        /// <summary>
        /// ジェスチャー推定を終了する
        /// </summary>  
        public void StopGestureEstimation()
        {
            TofArFaceManager.OnFaceEstimated -= TofArFaceManager_OnFaceEstimated;
            this.isEstimating = false;
        }

        private void TofArFaceManager_OnFaceEstimated(FaceResults faceResults)
        {

            if (faceResults.results.Length > FacialExpressionDetectionFaceIndex)
            {
                var faceResult = faceResults.results[FacialExpressionDetectionFaceIndex];

                // don't estimate expressions of frame with same timestamp as the last one
                if (lastFaceTimestamp == faceResult.timestamp)
                {
                    return;
                }

                TofArFacialExpressionEstimator.Instance.EstimateExpressions(faceResult);
            }
        }

        internal void EstimateExpressions(FaceResult face)
        {
            if (vowelDetector == null)
            {
                return;
            }

            FaceLogic.EstimateExpressionsInternal(ref face, ref isEstimating, ref input, ref facialExpressionsResult,
                ref openedEye, ref closedEye, ref upBrow, ref downBrow,
                ref OnFacialExpressionEstimated, ref lastFaceTimestamp, () => { return vowelDetector.forward(); });

        }

        public void GetMappedBlendshapes(ref FaceResult face)
        {
            if (vowelDetector == null)
            {
                return;
            }

            FaceLogic.GetMappedBlendshapesInternal(ref face, ref isEstimating, ref input, ref facialExpressionsResult,
                ref openedEye, ref closedEye, ref upBrow, ref downBrow, ref coeffs_a, ref coeffs_i, ref coeffs_u, ref coeffs_e, ref coeffs_o,
                ref OnFacialExpressionEstimated, ref lastFaceTimestamp, () => { return vowelDetector.forward(); });
        }

        internal void EstimateExpressions(FaceResults faceResults)
        {
            if (!isEstimating)
            {
                return;
            }

            if (vowelDetector == null)
            {
                return;
            }

            var faceData = faceResults;

            if (faceData == null)
            {
                return;
            }

            if (faceData.results.Length > FacialExpressionDetectionFaceIndex)
            {
                var face = faceData.results[FacialExpressionDetectionFaceIndex];

                EstimateExpressions(face);
            }


        }

        private bool Init()
        {
            string networkVowelDetector = null;
            try
            {
                networkVowelDetector = LoadFileFromResources(networkName);

                vowelDetector = new TFLiteRuntime(networkVowelDetector, this.execMode, this.threadsNum);
            }
            catch (Exception e)
            {
                TofArManager.Logger.WriteLog(LogLevel.Debug, $"[FacialExpressionEstimator Init] Failed to initialize TFLite: {e.Message}");
            }
            finally
            {
                if (networkVowelDetector != null && File.Exists(networkVowelDetector))
                {
                    File.Delete(networkVowelDetector);
                }
            }

            if (vowelDetector == null)
            {
                TofArManager.Logger.WriteLog(LogLevel.Debug, $"Deactivating FacialExpressionEstimator");
                return false;
            }
            TofArManager.Logger.WriteLog(LogLevel.Debug, $"[FacialExpressionEstimator Init] success");

            input = vowelDetector.getInputBuffer()[0];

            return true;
        }


        private string LoadFileFromResources(string filePath)
        {
            string asset_path = null;
            string local_path = null;

            try
            {
                asset_path = filePath;
                local_path = Application.persistentDataPath + "/" + asset_path;

                FileInfo file_info = new FileInfo(local_path);
                file_info.Directory.Create();

                TextAsset asset = Resources.Load(asset_path) as TextAsset;
                if (asset != null)
                {
                    Stream s = new MemoryStream(asset.bytes);
                    BinaryReader br = new BinaryReader(s);
                    File.WriteAllBytes(local_path, br.ReadBytes(asset.bytes.Length));
                }
            }
            catch (ArgumentException e)
            {
                TofArManager.Logger.WriteLog(LogLevel.Debug, $"Failed to load file from StreamingAssets:\n    asset_path = {asset_path}\n    local_path = {local_path}. Reason {e.Message}");
                throw;
            }
            catch (IOException e)
            {
                TofArManager.Logger.WriteLog(LogLevel.Debug, $"Failed to load file from StreamingAssets:\n    asset_path = {asset_path}\n    local_path = {local_path}. Reason {e.Message}");
                throw;
            }
            catch (System.Net.WebException e)
            {
                TofArManager.Logger.WriteLog(LogLevel.Debug, $"Failed to load file from StreamingAssets:\n    asset_path = {asset_path}\n    local_path = {local_path}. Reason {e.Message}");
                throw;
            }
            catch (TimeoutException e)
            {
                TofArManager.Logger.WriteLog(LogLevel.Debug, $"Failed to load file from StreamingAssets:\n    asset_path = {asset_path}\n    local_path = {local_path}. Reason {e.Message}");
                throw;
            }

            return local_path;
        }
    }
}
