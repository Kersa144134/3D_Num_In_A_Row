// ======================================================
// BaseUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : UI エフェクトの描画処理を担当するビュー
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using ShaderSystem.Application;

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

        /// <summary>ポインター Image 配列</summary>
        protected Image[] _pointerImages;

        /// <summary>Canvas RectTransform</summary>
        protected RectTransform _canvasRect;

        /// <summary>通常フォーカス時カラー</summary>
        protected Color _normalFocusOnColor;

        /// <summary>通常非フォーカス時カラー</summary>
        protected Color _normalFocusOffColor;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 通常ボタン に紐づく Image キャッシュ
        /// </summary>
        protected readonly Dictionary<Button, Image> _normalButtonImageCache =
            new Dictionary<Button, Image>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(
            in ScriptableRendererFeature binFeature,
            in Material binMaterial,
            in ScriptableRendererFeature greyFeature,
            in Material greyMaterial,
            in ScriptableRendererFeature disFeature,
            in Material disMaterial,
            in GameObject pointer,
            in Color normalFocusOnColor,
            in Color normalFocusOffColor)
        {
            _pointer = pointer;
            _normalFocusOnColor = normalFocusOnColor;
            _normalFocusOffColor = normalFocusOffColor;

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
                _pointerImages = _pointer.GetComponentsInChildren<Image>(true);

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
        /// 全画面エフェクトを更新する
        /// </summary>
        public void UpdateFullScreenEffect(
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
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// 通常ボタンのフォーカス状態を更新する
        /// </summary>
        /// <param name="button">対象ボタン</param>
        /// <param name="isFocus">フォーカス状態</param>
        public void SetNormalFocus(in Button button, in bool isFocus)
        {
            // 通常ボタン辞書へ登録
            RegisterButtonImageCache(button, _normalButtonImageCache);

            SetFocusState(
                button,
                isFocus,
                _normalButtonImageCache,
                _normalFocusOnColor,
                _normalFocusOffColor);
        }
        
        // --------------------------------------------------
        // ポインター
        // --------------------------------------------------
        /// <summary>
        /// ポインターの表示状態を切り替える
        /// </summary>
        /// <param name="isVisible">表示する場合は true</param>
        public void SetPointerVisible(in bool isVisible)
        {
            if (_pointerImages == null)
            {
                return;
            }
            Debug.Log(_pointerImages.Length);
            foreach(Image image in _pointerImages)
            {
                if (image == null)
                {
                    continue;
                }

                image.enabled = isVisible;
            }
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