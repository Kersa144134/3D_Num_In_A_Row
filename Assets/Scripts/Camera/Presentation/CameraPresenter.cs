// ======================================================
// CameraPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-08
// 更新日時 : 2026-05-18
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

        [Header("移動範囲設定")]
        /// <summary>Z 距離の最小値</summary>
        [SerializeField]
        private float _minDistanceZ = 1.0f;

        /// <summary>Z 距離の最大値</summary>
        [SerializeField]
        private float _maxDistanceZ = 2.0f;

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

        /// <summary>OrthographicSize の最小値</summary>
        [SerializeField]
        private float _orthographicSizeMin = 0.5f;

        /// <summary>OrthographicSize の最大値</summary>
        [SerializeField]
        private float _orthographicSizeMax = 1.0f;

        /// <summary>NearClip</summary>
        [SerializeField]
        private float _nearClip = 0.1f;

        /// <summary>FarClip</summary>
        [SerializeField]
        private float _farClip = 100.0f;

        [Header("追従オブジェクト")]
        /// <summary>カメラに追従する GameObject 配列</summary>
        [SerializeField]
        private GameObject[] _cameraFollowObjects;

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

        /// <summary>距離ユースケース</summary>
        private CameraDistanceUseCase _distanceUseCase;

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

        /// <summary>カメラ Transform 参照</summary>
        private Transform _cameraTransform;

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

        /// <summary>ゲームパッド使用中フラグ</summary>
        private bool _isGamepadUsed = false;

        /// <summary>入力用の距離変更最大速度（度 / 秒）</summary>
        private float _maxDistanceSpeed;

        /// <summary>入力用の距離変更加速度（度 / 秒の2乗）</summary>
        private float _distanceAcceleration;

        /// <summary>入力用の回転最大速度（度 / 秒）</summary>
        private float _maxRotationSpeed;

        /// <summary>入力用の回転加速度（度 / 秒の2乗）</summary>
        private float _rotationAcceleration;

        /// <summary>ロック前の X 回転キャッシュ</summary>
        private float _cachedRotationX;

        /// <summary>ロック前の Y 回転キャッシュ</summary>
        private float _cachedRotationY;

        /// <summary>成立ライン中心座標</summary>
        private Vector3 _centerPosition = Vector3.zero;

        /// <summary>成立ライン中心差分ベクトル</summary>
        private Vector3 _centerOffsetVector = Vector3.zero;

        /// <summary>成立ライン中心座標からの目標 Z 距離</summary>
        private float _targetCenterZDistance = DEFAULT_CAMERA_Z_DISTANCE;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 距離
        // --------------------------------------------------
        /// <summary>入力用 Z 距離基準速度倍率</summary>
        private const float INPUT_DISTANCE_BASE_SPEED_MULTIPLIER = 0.01f;

        /// <summary>入力用 Z 距離加速度倍率</summary>
        private const float INPUT_DISTANCE_ACCELERATION_MULTIPLIER = 4f;

        /// <summary>カメラ Z 座標距離の基準値</summary>
        private const float DEFAULT_CAMERA_Z_DISTANCE = 2f;

        /// <summary>ライン成立時のカメラ Z 座標距離</summary>
        private const float LINE_COMPLETE_CAMERA_Z_DISTANCE = 1.25f;

        // --------------------------------------------------
        // 回転
        // --------------------------------------------------
        /// <summary>入力用回転加速度倍率</summary>
        private const float INPUT_ROTATION_ACCELERATION_MULTIPLIER = 2f;

        /// <summary>イベント用回転収束時間</summary>
        private const float EVENT_SMOOTH_TIME = 0.2f;

        /// <summary>真上俯瞰角のカメラ回転</summary>
        private const float CAMERA_ROTATION_TOP_VIEW_X = 90.0f;

        /// <summary>成立ライン中心差分の基準ベクトル</summary>
        private static readonly Vector3 DEFAULT_CENTER_OFFSET_VECTOR = new Vector3(1.0f, 0.0f, 0f);

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
            _cameraTransform = _camera.transform;

            if (_gameOptionManager == null ||
                _inputManager == null ||
                _camera == null ||
                _cameraTransform == null)
            {
                Debug.LogError("[CameraPresenter] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            // オプションから速度計算に使用する数値を算出
            _maxDistanceSpeed = _gameOptionManager.CameraSpeed * INPUT_DISTANCE_BASE_SPEED_MULTIPLIER;
            _distanceAcceleration = _maxDistanceSpeed * INPUT_DISTANCE_ACCELERATION_MULTIPLIER;

            _maxRotationSpeed = _gameOptionManager.CameraSpeed;
            _rotationAcceleration = _maxRotationSpeed * INPUT_ROTATION_ACCELERATION_MULTIPLIER;

            // 現在の Transform の回転を取得する
            Vector3 euler = transform.rotation.eulerAngles;

            // X 回転を -180 ～ 180 に正規化する
            float initialRotationX = _angleUtility.NormalizeAngle(euler.x);

            // Y 回転を -180 ～ 180 に正規化する
            float initialRotationY = _angleUtility.NormalizeAngle(euler.y);

            // モデル、ビュー初期化
            _cameraModel = new CameraModel(
                DEFAULT_CAMERA_Z_DISTANCE,
                initialRotationX,
                initialRotationY,
                _minDistanceZ,
                _maxDistanceZ,
                _minRotationX,
                _maxRotationX,
                _orthographicSizeMin,
                _orthographicSizeMax
            );
            _cameraView = new CameraView(transform, _cameraTransform);

            // 初期化
            _projectionService = new CameraProjectionService(
                _perspectiveFov,
                _orthographicSizeMax,
                _nearClip,
                _farClip
            );
            _distanceUseCase = new CameraDistanceUseCase(
                _cameraModel,
                _maxDistanceSpeed,
                _distanceAcceleration,
                EVENT_SMOOTH_TIME
            );
            _rotationUseCase = new CameraRotationUseCase(
                _cameraModel,
                _maxRotationSpeed,
                _rotationAcceleration,
                EVENT_SMOOTH_TIME
            );
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            // 入力ロック中はイベントによる回転
            if (_isInputLock)
            {
                // イベント Z 距離
                UpdateEventDistance(_targetCenterZDistance, unscaledDeltaTime);

                // イベント回転
                UpdateEventRotation(_centerOffsetVector, unscaledDeltaTime);

                return;
            }

            // DPad Vertical 入力取得
            float dpadVerticdal = _inputManager.DPad.Angle.y;

            // 左スティック入力取得
            float leftStickHorizontal = _inputManager.LeftStick.Angle.x;
            float leftStickVertical = _inputManager.LeftStick.Angle.y;
            Vector2 leftStickInput = new Vector2(leftStickHorizontal, leftStickVertical);

            // 入力 Z 距離
            // 上入力で近づける、下入力で遠ざける処理にするため、入力を反転して渡す
            UpdateInputDistance(-dpadVerticdal, _isGamepadUsed, unscaledDeltaTime);

            // 入力回転
            UpdateInputRotation(leftStickInput, unscaledDeltaTime);
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
        /// 入力ロック状態およびボード回転準備関連ストリームをまとめて購読する
        /// </summary>
        /// <param name="inputLock">入力ロック状態通知ストリーム</param>
        /// <param name="gamepadUsed">ゲームパッド使用状態通知ストリーム</param>
        /// <param name="rotationPreparation">ボード回転準備状態通知ストリーム</param>
        /// <param name="centerPosition">成立ライン中心座標通知ストリーム</param>
        /// <param name="centerOffsetVector">成立ライン中心との差分ベクトル通知ストリーム</param>
        public void BindStreams(
            in IObservable<bool> inputLock,
            in IObservable<bool> gamepadUsed,
            in IObservable<bool> rotationPreparation,
            in IObservable<Vector3> centerPosition,
            in IObservable<Vector3> centerOffsetVector)
        {
            inputLock
                .Subscribe(isLock =>
                {
                    // 入力ロック状態を更新
                    _isInputLock = isLock;

                    // --------------------------------------------------
                    // ロック時
                    // --------------------------------------------------
                    if (_isInputLock)
                    {
                        // 回転速度を初期化
                        _rotationUseCase.ResetVelocity();

                        return;
                    }

                    // --------------------------------------------------
                    // ロック解除時
                    // --------------------------------------------------
                    // 目標値をリセット
                    _centerPosition = Vector3.zero;
                    _centerOffsetVector = Vector3.zero;
                    _targetCenterZDistance = DEFAULT_CAMERA_Z_DISTANCE;
                })
                .AddTo(_disposables);

            gamepadUsed
                .Subscribe(isUsed =>
                {
                    // ゲームパッド使用状態を更新
                    _isGamepadUsed = isUsed;
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

                        // 初期値を設定
                        _cameraModel.SetDistanceZ(_maxDistanceZ);

                        // キャッシュを復元
                        _cameraModel.SetRotationX(_cachedRotationX);
                        _cameraModel.SetRotationY(_cachedRotationY);

                        // ビュー反映
                        _cameraView.ApplyDistanceZ(_camera, _cameraModel.DistanceZ, _cameraModel.OrthographicSize);
                        _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);

                        // カメラ投影設定を更新
                        _projectionService.SetProjection(_camera, isPerspective);

                        return;
                    }

                    // --------------------------------------------------
                    // 透視投影切り替え時
                    // --------------------------------------------------
                    // カメラ入力ロック
                    _isInputLock = true;

                    // 現在値をキャッシュ
                    _cachedRotationX = _cameraModel.RotationX;
                    _cachedRotationY = _cameraModel.RotationY;

                    // 真上視点用の距離、回転へ変更
                    _cameraModel.SetDistanceZ(_maxDistanceZ);
                    _cameraModel.SetRotationX(CAMERA_ROTATION_TOP_VIEW_X);
                    _cameraModel.SetRotationY(0f);

                    // ビュー反映
                    _cameraView.ApplyDistanceZ(_camera, _cameraModel.DistanceZ, _cameraModel.OrthographicSize);
                    _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);

                    // カメラ投影設定を更新
                    _projectionService.SetProjection(_camera, isPerspective);
                })
                .AddTo(_disposables);

            centerPosition
                .Subscribe(centerPosition =>
                {
                    _centerPosition = centerPosition;
                    _targetCenterZDistance = LINE_COMPLETE_CAMERA_Z_DISTANCE;
                })
                .AddTo(_disposables);

            centerOffsetVector
                .Subscribe(centerOffsetVector =>
                {
                    _centerOffsetVector = centerOffsetVector;

                    if (_centerOffsetVector == Vector3.zero)
                    {
                        _centerOffsetVector = DEFAULT_CENTER_OFFSET_VECTOR;
                    }
                })
                .AddTo(_disposables);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 入力によるカメラ回転処理
        /// </summary>
        /// <param name="input">入力値</param>
        /// <param name="isGamepadUsed">ゲームパッドを使用しているか</param>
        /// <param name="unscaledDeltaTime">非スケールデルタ時間</param>
        private void UpdateInputDistance(in float input, in bool isGamepadUsed, in float unscaledDeltaTime)
        {
            // Z 距離更新
            _distanceUseCase.UpdateInputDistance(input, isGamepadUsed, unscaledDeltaTime);

            // ビュー反映
            _cameraView.ApplyDistanceZ(_camera, _cameraModel.DistanceZ, _cameraModel.OrthographicSize);

            // 追従オブジェクトのスケール更新
            UpdateFollowObjectScale();
        }

        /// <summary>
        /// イベントによるカメラ距離更新処理
        /// </summary>
        /// <param name="targetDistanceZ">目標 Z 距離</param>
        /// <param name="unscaledDeltaTime">非スケールデルタ時間</param>
        private void UpdateEventDistance(
            in float targetDistanceZ,
            in float unscaledDeltaTime)
        {
            // カメラと成立ライン中心座標間の距離を算出
            float currentDistance = Vector3.Distance(
                _cameraTransform.position,
                _centerPosition);

            // --------------------------------------------------
            // 距離補正量算出
            // --------------------------------------------------
            // 現在距離と目標距離との差分を算出
            float distanceDifference = currentDistance - targetDistanceZ;

            // 現在のモデル距離から差分を減算
            float correctedDistanceZ = Mathf.Abs(_cameraModel.DistanceZ) - distanceDifference;

            // 距離更新
            _distanceUseCase.UpdateEventDistance(
                correctedDistanceZ,
                unscaledDeltaTime);

            // ビュー反映
            _cameraView.ApplyDistanceZ(_camera, _cameraModel.DistanceZ, _cameraModel.OrthographicSize);

            // 追従オブジェクトのスケール更新
            UpdateFollowObjectScale();
        }
        
        /// <summary>
        /// 入力によるカメラ回転処理
        /// </summary>
        /// <param name="input">入力値</param>
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
        /// <param name="targetAngle">目標アングル</param>
        /// <param name="unscaledDeltaTime">非スケールデルタ時間</param>
        private void UpdateEventRotation(in Vector3 targetAngle, in float unscaledDeltaTime)
        {
            // 回転更新
            _rotationUseCase.UpdateEventRotation(targetAngle, unscaledDeltaTime);

            // ビュー反映
            _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
        }

        /// <summary>
        /// 追従オブジェクトのスケール更新
        /// </summary>
        private void UpdateFollowObjectScale()
        {
            if (_cameraFollowObjects == null)
            {
                return;
            }

            // 距離値をスケール値として使用
            Vector3 scale = Vector3.one * _cameraModel.DistanceZ;

            for (int i = 0; i < _cameraFollowObjects.Length; i++)
            {
                if (_cameraFollowObjects[i] == null)
                {
                    continue;
                }

                // スケール反映
                _cameraFollowObjects[i].transform.localScale = scale;
            }
        }
    }
}