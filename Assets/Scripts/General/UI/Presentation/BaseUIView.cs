// ======================================================
// BaseUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : UI エフェクトの描画処理を担当するビュー
// ======================================================

using ShaderSystem.Application;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace UISystem.Presentation
{
    /// <summary>
    /// UI エフェクトの描画を担当するビュー
    /// </summary>
    public abstract class BaseUIView
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>2 値化エフェクト制御クラス</summary>
        private BinarizationPostProcessController _binarization;

        /// <summary>グレースケール制御クラス</summary>
        private GreyScalePostProcessController _greyScale;

        /// <summary>歪み制御クラス</summary>
        private DistortionPostProcessController _distortion;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ポインター</summary>
        protected GameObject _pointer;

        /// <summary>ポインター RectTransform</summary>
        protected RectTransform _pointerRect;

        /// <summary>Canvas RectTransform</summary>
        protected RectTransform _canvasRect;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 基底クラスの初期化
        /// </summary>
        public void InitializeBase(
            in ScriptableRendererFeature binFeature,
            in Material binMaterial,
            in ScriptableRendererFeature greyFeature,
            in Material greyMaterial,
            in ScriptableRendererFeature disFeature,
            in Material disMaterial,
            in GameObject pointer)
        {
            _pointer = pointer;

            // --------------------------------------------------
            // エフェクト初期化
            // --------------------------------------------------
            _binarization = new BinarizationPostProcessController(binFeature, binMaterial);
            _greyScale = new GreyScalePostProcessController(greyFeature, greyMaterial);
            _distortion = new DistortionPostProcessController(disFeature, disMaterial);

            // --------------------------------------------------
            // ポインター初期化
            // --------------------------------------------------
            if (_pointer != null)
            {
                _pointerRect = _pointer.GetComponent<RectTransform>();

                // 親 Canvas を取得
                Canvas canvas = _pointer.GetComponentInParent<Canvas>();

                if (canvas != null)
                {
                    _canvasRect = canvas.transform as RectTransform;
                }
            }
        }

        // --------------------------------------------------
        // エフェクト
        // --------------------------------------------------
        /// <summary>
        /// エフェクトを更新する
        /// </summary>
        public void UpdateEffect(
            bool binEnabled,
            Vector2 binCenter,
            float binDistortion,
            float binNoise,
            float binThreshold,
            Color binLight,
            Color binDark,
            bool greyEnabled,
            Vector3 greyStrength,
            Vector2 greyCenter,
            float greyDistortion,
            float greyNoise,
            Color greyLight,
            Color greyDark,
            bool disEnabled,
            Vector2 disCenter,
            float disStrength,
            float disNoise)
        {
            // 2 値化エフェクトの状態とパラメータを反映する
            _binarization?.Update(
                binEnabled,
                binCenter,
                binDistortion,
                binNoise,
                binThreshold,
                binLight,
                binDark
            );

            // グレースケールエフェクトの状態とパラメータを反映する
            _greyScale?.Update(
                greyEnabled,
                greyStrength,
                greyCenter,
                greyDistortion,
                greyNoise,
                greyLight,
                greyDark
            );

            // 歪みエフェクトの状態とパラメータを反映する
            _distortion?.Update(
                disEnabled,
                disCenter,
                disStrength,
                disNoise
            );
        }

        // --------------------------------------------------
        // ポインター
        // --------------------------------------------------
        /// <summary>
        /// ポインターの表示状態を切り替える
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        public void SetPointerVisible(in bool isVisible)
        {
            if (_pointer == null)
            {
                return;
            }

            _pointer.SetActive(isVisible);
        }

        /// <summary>
        /// ポインター位置更新
        /// </summary>
        public void UpdatePointer(in Vector2 screenPosition)
        {
            if (_pointerRect == null || _canvasRect == null)
            {
                return;
            }

            // Canvas中心基準へ変換
            Vector2 anchoredPos = screenPosition - (_canvasRect.sizeDelta * 0.5f);

            // 位置反映
            _pointerRect.anchoredPosition = anchoredPos;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// Button と Image の対応情報を登録する
        /// </summary>
        /// <param name="button">対象ボタン</param>
        /// <param name="cacheDictionary">登録先辞書</param>
        protected void RegisterButtonImageCache(
            in Button button,
            in Dictionary<Button, Image> cacheDictionary)
        {
            if (button == null)
            {
                return;
            }

            // 既に登録済みの場合は処理なし
            if (cacheDictionary.ContainsKey(button))
            {
                return;
            }

            // Button 本体の Image を取得
            Image image = button.image;

            if (image == null)
            {
                return;
            }

            cacheDictionary.Add(button, image);
        }

        /// <summary>
        /// 指定ボタンのフォーカス状態を更新する
        /// </summary>
        /// <param name="button">対象ボタン</param>
        /// <param name="isFocus">フォーカス状態</param>
        /// <param name="cacheDictionary">対象辞書</param>
        /// <param name="focusOnColor">フォーカス ON 時の色</param>
        /// <param name="focusOffColor">対象ボタンの OFF 色</param>
        protected virtual void SetFocusState(
            in Button button,
            in bool isFocus,
            in Dictionary<Button, Image> cacheDictionary,
            in Color focusOnColor,
            in Color focusOffColor)
        {
            if (button == null)
            {
                return;
            }

            // --------------------------------------------------
            // 指定辞書のフォーカス状態更新
            // --------------------------------------------------
            foreach (KeyValuePair<Button, Image> cache in cacheDictionary)
            {
                if (cache.Value == null)
                {
                    continue;
                }

                // 対象ボタンの場合
                if (cache.Key == button)
                {
                    // フォーカス状態に応じた色を設定
                    cache.Value.color = isFocus
                        ? focusOnColor
                        : focusOffColor;

                    continue;
                }

                // 対象外ボタンはフォーカス OFF 状態へ変更
                cache.Value.color = focusOffColor;
            }
        }
    }
}