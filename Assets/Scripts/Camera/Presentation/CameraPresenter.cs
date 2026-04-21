// ======================================================
// CameraPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-08
// 更新日時 : 2026-04-20
// 概要     : カメラ入力・状態更新・描画反映を管理するプレゼンター
// ======================================================

using System;
using UnityEngine;
using UniRx;
using CameraSystem.Application;
using CameraSystem.Domain;
using InputSystem;
using PhaseSystem.Domain;
using SceneSystem.Domain;

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

        [Header("回転補間設定")]
        /// <summary>回転の最大速度（度 / 秒）補間時の上限速度として使用される</summary>
        [SerializeField]
        private float _maxRotationSpeed = 360.0f;

        /// <summary>回転の加速度（度 / 秒の2乗）</summary>
        [SerializeField]
        private float _rotationAcceleration = 720.0f;

        [Header("回転制限設定")]
        /// <summary>X 回転の最小値</summary>
        [SerializeField]
        private float _minRotationX = -90.0f;

        /// <summary>X 回転の最大値</summary>
        [SerializeField]
        private float _maxRotationX = 90.0f;

        [Header("投影設定")]
        /// <summary>透視時の視野角</summary>
        [SerializeField]
        private float _perspectiveFov = 60.0f;

        /// <summary>平行時のサイズ</summary>
        [SerializeField]
        private float _orthographicSize = 0.75f;

        /// <summary>NearClip</summary>
        [SerializeField]
        private float _nearClip = 0.1f;

        /// <summary>FarClip</summary>
        [SerializeField]
        private float _farClip = 100.0f;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>モデル</summary>
        private CameraModel _cameraModel;

        /// <summary>ビュー</summary>
        private CameraView _cameraView;

        /// <summary>角度計算ユーティリティ</summary>
        private readonly CameraAngleUtility _angleUtility = new CameraAngleUtility();

        /// <summary>回転ユースケース</summary>
        private CameraRotationUseCase _rotationUseCase;

        /// <summary>投影補間サービス</summary>
        private CameraProjectionService _projectionService;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>カメラ参照</summary>
        private Camera _camera;

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
            // カメラ取得
            _camera = Camera.main;
            
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

            // 初期化
            _projectionService = new CameraProjectionService(
                _perspectiveFov,
                _orthographicSize,
                _nearClip,
                _farClip
            );
            _rotationUseCase = new CameraRotationUseCase(
                _cameraModel,
                _maxRotationSpeed,
                _rotationAcceleration
            );

            // InputManagerのシングルトンインスタンスを取得
            _inputManager = InputManager.Instance;
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            // --------------------------------------------------
            // 回転処理
            // --------------------------------------------------
            // 入力がロックされている場合は処理を行わない
            if (_isInputLock)
            {
                // 入力無効のため早期リターン
                return;
            }

            // 左右入力を取得する
            float inputHorizontal = _inputManager.LeftStick.x;

            // 上下入力を取得する
            float inputVertical = _inputManager.LeftStick.y;

            // 入力値を Vector2 としてまとめる
            Vector2 input = new Vector2(inputHorizontal, inputVertical);

            // 入力とデルタ時間を渡して回転をモデルへ反映する
            _rotationUseCase.UpdateRotation(input, unscaledDeltaTime);

            // モデルの回転値をビューへ適用する
            _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
        }

        // ======================================================
        // パブリックメソッド
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

        /// <summary>
        /// 投影方式を透視または平行に切り替える
        /// </summary>
        /// <param name="isPerspective">true:透視 / false:平行</param>
        public void SwitchProjection(in bool isPerspective)
        {
            _projectionService.SetProjection(_camera, isPerspective);
        }
    }
}