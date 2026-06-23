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
using BoardSystem.Domain;
using CameraSystem.Application;
using CameraSystem.Domain;
using InputSystem.Presentation;
using OptionSystem.Presentation;
using SoundSystem.Domain;
using SoundSystem.Presentation;
using UpdateSystem.Domain;

namespace CameraSystem.Presentation
{
    /// <summary>
    /// カメラ制御用プレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.CameraPresenter)]
    public sealed class CameraPresenter : MonoBehaviour, IUpdatable, IStreamBindable
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

        /// <summary>目標位置、角度計算クラス</summary>
        private readonly CameraTargetCalculator _targetCalculator
            = new CameraTargetCalculator();

        /// <summary>投影ユースケース</summary>
        private CameraProjectionUseCase _projectionUseCase;

        /// <summary>位置ユースケース</summary>
        private CameraPositionUseCase _positionUseCase;

        /// <summary>回転ユースケース</summary>
        private CameraRotationUseCase _rotationUseCase;

        /// <summary>距離ユースケース</summary>
        private CameraDistanceUseCase _distanceUseCase;

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        /// <summary>SoundManager キャッシュ</summary>
        private SoundManager _soundManager;

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

        /// <summary>カメラ目標位置</summary>
        private Vector3 _targetPosition = Vector3.zero;

        /// <summary>カメラ目標角度</summary>
        private Vector3 _targetAngle = Vector3.zero;

        /// <summary>成立ライン中心座標からの目標 Z 距離</summary>
        private float _targetDistance = DEFAULT_CAMERA_Z_DISTANCE;

        /// <summary>回転 SE 再生済みフラグ</summary>
        private bool _isRotationSePlayed;

        /// <summary>ズーム SE 再生済みフラグ</summary>
        private bool _isZoomSePlayed;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // 回転
        // --------------------------------------------------
        /// <summary>入力用回転加速度倍率</summary>
        private const float INPUT_ROTATION_ACCELERATION_MULTIPLIER = 2f;

        /// <summary>イベント用回転収束時間</summary>
        private const float EVENT_SMOOTH_TIME = 0.2f;

        /// <summary>真上俯瞰角のカメラ回転</summary>
        private const float CAMERA_ROTATION_TOP_VIEW_X = 90.0f;

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
        private const float LINE_COMPLETE_CAMERA_Z_DISTANCE = 1.0f;

        // --------------------------------------------------
        // サウンド
        // --------------------------------------------------
        /// <summary>SE 判定用の速度しきい値</summary>
        private const float SE_VELOCITY_THRESHOLD = 0.001f;
        
        // ======================================================
        // UniRx 関連
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private CompositeDisposable _disposables;

        /// <summary>カメラ位置リセット通知ストリーム</summary>
        private IObservable<Unit> _positionResetStream;

        /// <summary>入力ロック状態通知ストリーム</summary>
        private IObservable<bool> _inputLockStream;

        /// <summary>ゲームパッド使用状態通知ストリーム</summary>
        private IObservable<bool> _gamepadUsedStream;

        /// <summary>ボード回転準備状態通知ストリーム</summary>
        private IObservable<bool> _rotationPreparationStream;

        /// <summary>ライン位置通知ストリーム</summary>
        private IObservable<LinePositionInfo> _linePositionStream;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;
            _soundManager = SoundManager.Instance;

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
                Vector3.zero,
                initialRotationX,
                initialRotationY,
                DEFAULT_CAMERA_Z_DISTANCE,
                _minRotationX,
                _maxRotationX,
                _minDistanceZ,
                _maxDistanceZ,
                _orthographicSizeMin,
                _orthographicSizeMax
            );
            _cameraView = new CameraView(transform, _cameraTransform);

            // 初期化
            _positionUseCase = new CameraPositionUseCase(
                _cameraModel,
                EVENT_SMOOTH_TIME
            );
            _rotationUseCase = new CameraRotationUseCase(
                _cameraModel,
                _maxRotationSpeed,
                _rotationAcceleration,
                EVENT_SMOOTH_TIME
            );
            _distanceUseCase = new CameraDistanceUseCase(
                _cameraModel,
                _maxDistanceSpeed,
                _distanceAcceleration,
                EVENT_SMOOTH_TIME
            );
            _projectionUseCase = new CameraProjectionUseCase(
                _perspectiveFov,
                _orthographicSizeMax,
                _nearClip,
                _farClip
            );

        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            if (_isInputLock)
            {
                // --------------------------------------------------
                // イベント更新
                // --------------------------------------------------
                UpdateEventPosition(_targetPosition, unscaledDeltaTime);
                UpdateEventRotation(_targetAngle, unscaledDeltaTime);
                UpdateEventDistance(_targetDistance, unscaledDeltaTime);

                return;
            }

            // --------------------------------------------------
            // 入力更新
            // --------------------------------------------------
            // DPad Vertical 入力取得
            float dpadVertical = _inputManager.DPad.Angle.y;

            // 左スティック入力取得
            float leftStickHorizontal = _inputManager.LeftStick.Angle.x;
            float leftStickVertical = _inputManager.LeftStick.Angle.y;
            Vector2 leftStickInput = new Vector2(leftStickHorizontal, leftStickVertical);

            // DPad
            // 上入力で近づける、下入力で遠ざける処理にするため入力を反転
            UpdateInputDistance(-dpadVertical, _isGamepadUsed, unscaledDeltaTime);

            // 左スティック
            UpdateInputRotation(leftStickInput, unscaledDeltaTime);

            // --------------------------------------------------
            // 回転 SE 再生
            // --------------------------------------------------
            // カメラの回転速度を取得
            Vector2 rotationVelocity = new Vector2(
                _rotationUseCase.VelocityX,
                _rotationUseCase.VelocityY);

            // 速度の大きさを取得
            float velocityMagnitude = rotationVelocity.magnitude;

            // 微小速度は停止扱いとする
            if (velocityMagnitude < SE_VELOCITY_THRESHOLD)
            {
                if (_isRotationSePlayed)
                {
                    _soundManager?.StopLoopSE(SeType.Camera_Rotation);

                    _isRotationSePlayed = false;
                }
            }
            else
            {
                // 未再生状態の場合のみ SE を再生
                if (!_isRotationSePlayed)
                {
                    _soundManager?.PlaySE(SeType.Camera_Rotation, 0.75f);

                    _isRotationSePlayed = true;
                }
            }

            // --------------------------------------------------
            // ズーム SE 再生
            // --------------------------------------------------
            // カメラの Z 方向移動速度を取得
            float distanceVelocity = _distanceUseCase.VelocityDistanceZ;

            // 微小速度は停止扱いとする
            if (Mathf.Abs(distanceVelocity) < SE_VELOCITY_THRESHOLD)
            {
                if (_isZoomSePlayed)
                {
                    _soundManager?.StopLoopSE(SeType.Camera_Zoom);

                    _isZoomSePlayed = false;
                }
            }
            else
            {
                // 未再生状態の場合のみ SE を再生
                if (!_isZoomSePlayed)
                {
                    _soundManager?.PlaySE(SeType.Camera_Zoom, 0.75f);

                    _isZoomSePlayed = true;
                }
            }
        }

        public void OnExit()
        {
            // イベント購読解除
            UnbindStreams();
        }

        // ======================================================
        // IStreamBindable イベント
        // ======================================================

        /// <summary>
        /// イベントストリームを受け取る
        /// </summary>
        public void BindStreams()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _positionResetStream
                .Subscribe(isLock =>
                {
                    _targetPosition = Vector3.zero;
                    _targetDistance = DEFAULT_CAMERA_Z_DISTANCE;
                })
                .AddTo(_disposables);

            _inputLockStream
                .Subscribe(isLock =>
                {
                    _isInputLock = isLock;

                    // --------------------------------------------------
                    // ロック時
                    // --------------------------------------------------
                    if (_isInputLock)
                    {
                        // 速度を初期化
                        _positionUseCase.ResetVelocity();
                        _rotationUseCase.ResetVelocity();
                        _distanceUseCase.ResetVelocity();

                        // --------------------------------------------------
                        // SE 停止
                        // --------------------------------------------------
                        if (_isRotationSePlayed)
                        {
                            _soundManager?.StopLoopSE(SeType.Camera_Rotation);

                            _isRotationSePlayed = false;
                        }
                        if (_isZoomSePlayed)
                        {
                            _soundManager?.StopLoopSE(SeType.Camera_Zoom);

                            _isZoomSePlayed = false;
                        }

                        return;
                    }

                    // --------------------------------------------------
                    // ロック解除時
                    // --------------------------------------------------
                    // 目標値をリセット
                    _targetPosition = Vector3.zero;
                    _targetAngle = Vector3.zero;
                    _targetDistance = DEFAULT_CAMERA_Z_DISTANCE;
                })
                .AddTo(_disposables);

            _gamepadUsedStream
                .Subscribe(isUsed => _isGamepadUsed = isUsed)
                .AddTo(_disposables);

            _rotationPreparationStream
                .Subscribe(isPerspective =>
                {
                    // SE 再生
                    _soundManager?.PlaySE(SeType.Camera_SwitchProjection, 0.75f);

                    // --------------------------------------------------
                    // 平行投影切り替え時
                    // --------------------------------------------------
                    if (!isPerspective)
                    {
                        // キャッシュを復元
                        _cameraModel.ApplyRotationX(_cachedRotationX);
                        _cameraModel.ApplyRotationY(_cachedRotationY);

                        // 初期値を設定
                        _cameraModel.ApplyDistanceZ(_maxDistanceZ);

                        // ビュー反映
                        _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
                        _cameraView.ApplyDistanceZ(_camera, _cameraModel.DistanceZ, _cameraModel.OrthographicSize);

                        // カメラ投影設定を更新
                        _projectionUseCase.ApplyProjection(_camera, isPerspective);

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
                    _cameraModel.ApplyRotationX(CAMERA_ROTATION_TOP_VIEW_X);
                    _cameraModel.ApplyRotationY(0f);
                    _cameraModel.ApplyDistanceZ(_maxDistanceZ);

                    // ビュー反映
                    _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
                    _cameraView.ApplyDistanceZ(_camera, _cameraModel.DistanceZ, _cameraModel.OrthographicSize);

                    // カメラ投影設定を更新
                    _projectionUseCase.ApplyProjection(_camera, isPerspective);
                })
                .AddTo(_disposables);

            _linePositionStream
                .Subscribe(linePosition =>
                {
                    // 目標位置更新
                    _targetCalculator.Calculate(
                        linePosition.StartPosition,
                        linePosition.EndPosition,
                        out _targetPosition,
                        out _targetAngle);

                    // 目標距離更新
                    _targetDistance = LINE_COMPLETE_CAMERA_Z_DISTANCE;
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// イベントストリームを受け取る
        /// </summary>
        public void UnbindStreams()
        {
            _disposables?.Dispose();

            // --------------------------------------------------
            // SE 停止
            // --------------------------------------------------
            if (_isRotationSePlayed)
            {
                _soundManager?.StopLoopSE(SeType.Camera_Rotation);

                _isRotationSePlayed = false;
            }
            if (_isZoomSePlayed)
            {
                _soundManager?.StopLoopSE(SeType.Camera_Zoom);

                _isZoomSePlayed = false;
            }
        }
        
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        /// <param name="positionReset">カメラ位置リセット通知ストリーム</param>
        /// <param name="inputLock">入力ロック状態通知ストリーム</param>
        /// <param name="gamepadUsed">ゲームパッド使用状態通知ストリーム</param>
        /// <param name="rotationPreparation">ボード回転準備状態通知ストリーム</param>
        /// <param name="linePosition">ライン位置通知ストリーム</param>
        public void SetStreams(
            in IObservable<Unit> positionReset,
            in IObservable<bool> inputLock,
            in IObservable<bool> gamepadUsed,
            in IObservable<bool> rotationPreparation,
            in IObservable<LinePositionInfo> linePosition)
        {
            _positionResetStream = positionReset;
            _inputLockStream = inputLock;
            _gamepadUsedStream = gamepadUsed;
            _rotationPreparationStream = rotationPreparation;
            _linePositionStream = linePosition;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // 位置
        // --------------------------------------------------
        /// <summary>
        /// イベントによるカメラ位置更新処理
        /// </summary>
        /// <param name="targetPosition">目標座標</param>
        /// <param name="unscaledDeltaTime">非スケールデルタ時間</param>
        private void UpdateEventPosition(
            in Vector3 targetPosition,
            in float unscaledDeltaTime)
        {
            // 位置更新
            _positionUseCase.UpdateEventPosition(targetPosition, unscaledDeltaTime);

            // ビュー反映
            _cameraView.ApplyPosition(_cameraModel.Position);
        }

        // --------------------------------------------------
        // 回転
        // --------------------------------------------------
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

        // --------------------------------------------------
        // 距離
        // --------------------------------------------------
        /// <summary>
        /// 入力によるカメラ距離更新処理
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
            // Z 距離更新
            _distanceUseCase.UpdateEventDistance(targetDistanceZ, unscaledDeltaTime);

            // ビュー反映
            _cameraView.ApplyDistanceZ(_camera, _cameraModel.DistanceZ, _cameraModel.OrthographicSize);

            // 追従オブジェクトのスケール更新
            UpdateFollowObjectScale();
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