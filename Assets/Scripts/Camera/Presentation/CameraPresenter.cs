// ======================================================
// CameraPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-08
// 更新日時 : 2026-04-20
// 概要     : カメラ入力・状態更新・描画反映を管理するプレゼンター
// ======================================================

using CameraSystem.Application;
using CameraSystem.Domain;
using InputSystem;
using PhaseSystem.Domain;
using SceneSystem.Domain;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace CameraSystem.Presentation
{
    /// <summary>
    /// カメラ制御用プレゼンター
    /// </summary>
    public sealed class CameraPresenter : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("回転速度設定")]
        /// <summary>水平回転速度</summary>
        [SerializeField]
        private float _rotationSpeedY = 120.0f;

        /// <summary>垂直回転速度</summary>
        [SerializeField]
        private float _rotationSpeedX = 120.0f;

        [Header("回転制限設定")]
        /// <summary>X 回転の最小値</summary>
        [SerializeField]
        private float _minRotationX = -90.0f;

        /// <summary>X 回転の最大値</summary>
        [SerializeField]
        private float _maxRotationX = 90.0f;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>モデル</summary>
        private CameraModel _cameraModel;

        /// <summary>ビュー</summary>
        private CameraView _cameraView;

        private readonly CameraAngleUtility _angleUtility = new CameraAngleUtility();

        /// <summary>回転ユースケース</summary>
        private CameraRotationUseCase _rotationUseCase;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>フェーズ購読保持</summary>
        private IDisposable _phaseSubscription;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // 現在の Transform の回転を取得する
            Vector3 euler = transform.rotation.eulerAngles;

            // X 回転を -180 ～ 180 に正規化する
            float initialRotationX = _angleUtility.NormalizeAngle(euler.x);

            // Y 回転を -180 ～ 180 に正規化する
            float initialRotationY = _angleUtility.NormalizeAngle(euler.y);

            // モデル、ビュー初期化
            _cameraModel = new CameraModel(
                initialRotationX,
                initialRotationY,
                _minRotationX,
                _maxRotationX
            );
            _cameraView = new CameraView(transform);

            // ユースケース初期化
            _rotationUseCase = new CameraRotationUseCase(
                _cameraModel,
                _rotationSpeedX,
                _rotationSpeedY
            );

            // InputManagerのシングルトンインスタンスを取得
            _inputManager = InputManager.Instance;
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            // 入力がロックされている場合は処理を行わない
            if (_isInputLock)
            {
                // 入力無効のため早期リターン
                return;
            }

            // --------------------------------------------------
            // 入力取得
            // --------------------------------------------------
            // 左右入力を取得する
            float inputHorizontal = _inputManager.LeftStick.x;

            // 上下入力を取得する
            float inputVertical = _inputManager.LeftStick.y;

            // 入力値を Vector2 としてまとめる
            Vector2 input = new Vector2(inputHorizontal, inputVertical);

            // --------------------------------------------------
            // ユースケース実行
            // --------------------------------------------------
            // 入力とデルタ時間を渡して回転をモデルへ反映する
            _rotationUseCase.UpdateRotation(input, unscaledDeltaTime);

            // --------------------------------------------------
            // ビュー反映
            // --------------------------------------------------
            // モデルの回転値をビューへ適用する
            _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
        }

        // ======================================================
        // フェーズ制御
        // ======================================================

        /// <summary>
        /// フェーズ変更ストリームを購読し、現在のフェーズに応じて入力の有効・無効を制御する
        /// </summary>
        /// <param name="stream">フェーズ種別を通知するストリーム</param>
        public void BindPhaseStream(in IObservable<PhaseType> stream)
        {
            // 多重購読防止
            _phaseSubscription?.Dispose();

            _phaseSubscription = stream
                .Subscribe(phase =>
                {
                    if (phase == PhaseType.Play)
                    {
                        // 入力ロック解除
                        _isInputLock = false;
                    }
                    else
                    {
                        // 入力ロック
                        _isInputLock = true;
                    }
                });
        }

        /// <summary>
        /// フェーズ変更ストリームの購読を解除する
        /// </summary>
        public void UnbindPhaseStream()
        {
            _phaseSubscription?.Dispose();
            _phaseSubscription = null;
        }
    }
}