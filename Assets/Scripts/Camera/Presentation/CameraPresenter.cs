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
using OptionSystem.Presentation;
using InputSystem.Presentation;
using UpdateSystem.Domain;

namespace CameraSystem.Presentation
{
    /// <summary>
    /// カメラ制御用プレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.CameraPresenter)]
    public sealed class CameraPresenter : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

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

        /// <summary>投影補間サービス</summary>
        private CameraProjectionService _projectionService;

        /// <summary>回転ユースケース</summary>
        private CameraRotationUseCase _rotationUseCase;

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>カメラ参照</summary>
        private Camera _camera;

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;
        
        /// <summary>回転の最大速度（度 / 秒）</summary>
        private float _maxRotationSpeed;

        /// <summary>入力用の回転加速度（度 / 秒の2乗）</summary>
        private float _acceleration;

        /// <summary>ロック前の X 回転キャッシュ</summary>
        private float _cachedRotationX;

        /// <summary>ロック前の Y 回転キャッシュ</summary>
        private float _cachedRotationY;

        /// <summary>成立ライン中心差分ベクトル</summary>
        private Vector3 _centerOffset = Vector3.zero;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// 入力用回転加速度の倍率
        /// </summary>
        private const float INPUT_ACCELERATION_MULTIPLIER = 2f;

        /// <summary>
        /// イベント用回転収束時間
        /// </summary>
        private const float EVENT_SMOOTH_TIME = 0.2f;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;

            // カメラ取得
            _camera = Camera.main;

            if (_gameOptionManager == null || _inputManager == null || _camera == null)
            {
                Debug.LogError("[CameraPresenter] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            // オプションを取得
            _maxRotationSpeed = _gameOptionManager.CameraRotationSpeed;
            _acceleration = _maxRotationSpeed * INPUT_ACCELERATION_MULTIPLIER;

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
                _acceleration,
                EVENT_SMOOTH_TIME
            );
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            // --------------------------------------------------
            // 入力取得
            // --------------------------------------------------
            // 左右入力を取得する
            float inputHorizontal = _inputManager.LeftStick.Angle.x;

            // 上下入力を取得する
            float inputVertical = _inputManager.LeftStick.Angle.y;

            // 入力値を Vector2 としてまとめる
            Vector2 input = new Vector2(inputHorizontal, inputVertical);

            // --------------------------------------------------
            // 回転処理
            // --------------------------------------------------
            // 入力ロック中は処理なし
            if (_isInputLock)
            {
                UpdateEventRotation(_centerOffset, unscaledDeltaTime);
                return;
            }

            UpdateInputRotation(input, unscaledDeltaTime);
        }

        public void OnExit()
        {
            // イベント購読解除
            _disposables?.Dispose();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 入力ロックおよびボード回転準備ストリームをまとめて購読する
        /// </summary>
        /// <param name="inputLock">入力ロック状態ストリーム　true:ロック / false:解除</param>
        /// <param name="rotationPreparation">ボード回転準備トリガーストリーム</param>
        public void BindStreams(
            IObservable<bool> inputLock,
            IObservable<bool> rotationPreparation,
            IObservable<Vector3> centerOffset)
        {
            inputLock
                .Subscribe(isLock =>
                {
                    // 入力ロック状態を更新
                    _isInputLock = isLock;

                    // --------------------------------------------------
                    // ロック時処理
                    // --------------------------------------------------
                    if (_isInputLock)
                    {
                        // 回転速度を初期化
                        _rotationUseCase.ResetRotationVelocity();

                        return;
                    }
                })
                .AddTo(_disposables);

            rotationPreparation
                .Subscribe(isPerspective =>
                {
                    // --------------------------------------------------
                    // 平行投影切り替え時
                    // --------------------------------------------------
                    if (!isPerspective)
                    {
                        // カメラ入力ロック解除
                        _isInputLock = false;

                        // 事前に保存した回転を復元
                        _cameraModel.SetRotationX(_cachedRotationX);
                        _cameraModel.SetRotationY(_cachedRotationY);

                        // ビュー反映
                        _cameraView.ApplyRotation(
                            _cameraModel.RotationX,
                            _cameraModel.RotationY);

                        // カメラ投影設定を更新
                        _projectionService.SetProjection(_camera, isPerspective);

                        return;
                    }

                    // --------------------------------------------------
                    // 透視投影切り替え時
                    // --------------------------------------------------
                    // カメラ入力ロック
                    _isInputLock = true;

                    // 現在の回転値をキャッシュ
                    _cachedRotationX = _cameraModel.RotationX;
                    _cachedRotationY = _cameraModel.RotationY;

                    // 真上視点用の回転へ変更
                    _cameraModel.SetRotationX(90f);
                    _cameraModel.SetRotationY(0f);

                    // ビュー反映
                    _cameraView.ApplyRotation(
                        _cameraModel.RotationX,
                        _cameraModel.RotationY);

                    // カメラ投影設定を更新
                    _projectionService.SetProjection(_camera, isPerspective);
                })
                .AddTo(_disposables);

            centerOffset
                .Subscribe(centerOffset =>
                {
                    _centerOffset = centerOffset;
                })
                .AddTo(_disposables);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 入力によるカメラ回転処理
        /// </summary>
        /// <param name="unscaledDeltaTime">非スケールデルタ時間</param>
        private void UpdateInputRotation(in Vector2 input, in float unscaledDeltaTime)
        {
            // 回転更新
            _rotationUseCase.UpdateInputRotation(input, unscaledDeltaTime);

            // ビュー反映
            _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
        }

        /// <summary>
        /// イベントによるカメラ回転処理
        /// </summary>
        /// <param name="unscaledDeltaTime">非スケールデルタ時間</param>
        private void UpdateEventRotation(in Vector3 angle, in float unscaledDeltaTime)
        {
            // 回転更新
            _rotationUseCase.UpdateEventRotation(angle, unscaledDeltaTime);

            // ビュー反映
            _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
        }
    }
}