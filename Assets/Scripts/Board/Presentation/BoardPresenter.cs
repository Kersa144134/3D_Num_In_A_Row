// ======================================================
// BoardPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-07
// 概要     : 3D 目並べゲームの盤面を制御するクラス
// ======================================================

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using BoardSystem.Application;
using BoardSystem.Domain;
using InputSystem.Presentation;
using PhaseSystem.Domain;
using UpdateSystem.Domain;

namespace BoardSystem.Presentation
{
    /// <summary>
    /// 目並べプレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.BoardPresenter)]
    public sealed class BoardPresenter : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("盤面")]
        /// <summary>
        /// 盤面サイズ
        /// </summary>
        [SerializeField, Min(3)]
        private int _boardSize;

        /// <summary>
        /// 勝利条件の連続駒数
        /// </summary>
        [SerializeField, Min(3)]
        private int _connectCount;

        /// <summary>
        /// 盤面の列選択表示ルート
        /// </summary>
        [SerializeField]
        private GameObject _columnSelectRoot;

        [Header("駒")]
        /// <summary>
        /// 駒の Prefab
        /// </summary>
        [SerializeField]
        private GameObject _piecePrefab;

        /// <summary>
        /// 駒のマテリアル配列
        /// </summary>
        [SerializeField]
        private Material[] _pieceMaterials;

        /// <summary>
        /// 駒削除時に再生するパーティクル
        /// </summary>
        [SerializeField]
        private GameObject _deleteParticle;

        /// <summary>
        /// 駒のスケール倍率
        /// </summary>
        [SerializeField, Range(0.5f, 1.0f)]
        private float _pieceScaleFactor;

        [Header("ヒット判定")]
        /// <summary>
        /// 盤面のクリック判定用 Collider
        /// </summary>
        [SerializeField]
        private Collider _boardCollider;

        /// <summary>
        /// Ray 判定用レイヤー
        /// </summary>
        [SerializeField]
        private LayerMask _raycastLayerMask;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>モデル</summary>
        private BoardModel _model;

        /// <summary>ビュー</summary>
        private BoardView _view;

        /// <summary>駒削除ハンドラ</summary>
        private BoardDeleteHandler _deleteHandler;

        /// <summary>駒落下ハンドラ</summary>
        private BoardDropHandler _dropHandler;

        /// <summary>盤面回転ユースケース</summary>
        private BoardRotationUseCase _rotationUseCase;

        /// <summary>駒再配置ユースケース</summary>
        private BoardRepositionUseCase _repositionUseCase;

        /// <summary>駒ビュー更新ハンドラ</summary>
        private BoardViewMoveHandler _viewMoveHandler;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>カメラ</summary>
        private Camera _camera;

        // RaycastHit配列
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        /// <summary>盤面の列選択表示プレーン</summary>
        private Transform _columnSelectPlane;

        /// <summary>現在のプレイヤーID</summary>
        private int _currentPlayer;

        /// <summary>落下実行中フラグ</summary>
        private bool _isDrop = false;

        /// <summary>回転実行中フラグ</summary>
        private bool _isRotate = false;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>プレイヤー未指定 ID</summary>
        private const int PLAYER_NONE = -1;

        /// <summary>Planeタグ</summary>
        private const string TAG_PLANE = "Plane";

        /// <summary>
        /// 駒削除後、落下処理を開始するまでの待機時間（ミリ秒）
        /// </summary>
        private const int PIECE_DROP_DELAY_MS = 500;

        /// <summary>
        /// ライン成立終了後、プレイヤー切り替えを開始するまでの待機時間（ミリ秒）
        /// </summary>
        private const int FINISH_LINE_CHECK_DELAY_MS = 1000;

        /// <summary>
        /// 盤面の回転アニメーション時間（秒）
        /// </summary>
        private const float ROTATION_DURATION = 0.5f;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>入力用イベント購読管理</summary>
        private CompositeDisposable _inputDisposables;

        /// <summary>プレイヤー入力通知用 Subject</summary>
        private readonly Subject<Unit> _onInputReceived = new Subject<Unit>();

        /// <summary>プレイヤー入力ストリーム</summary>
        public IObservable<Unit> OnInputReceived => _onInputReceived;

        /// <summary>ライン成立通知用 Subject</summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete =
            new Subject<LineCompleteEvent>();

        /// <summary>ライン成立ストリーム</summary>
        public IObservable<LineCompleteEvent> OnLineComplete => _onLineComplete;

        /// <summary>プレイヤー行動終了通知用 Subject</summary>
        private readonly Subject<Unit> _onPlayerEnd = new Subject<Unit>();

        /// <summary>プレイヤー行動終了ストリーム</summary>
        public IObservable<Unit> OnPlayerEnd => _onPlayerEnd;

        /// <summary>プレイヤーインデックス購読</summary>
        private IDisposable _playerIndexSubscription;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // インスタンスからコンポーネント取得
            _inputManager = InputManager.Instance;

            // シーン内のメインカメラを取得
            _camera = Camera.main;

            if (_inputManager == null || _camera ||
                _piecePrefab == null || _deleteParticle == null ||
                _boardCollider == null || _columnSelectRoot == null)
            {
                Debug.LogError("[BoardPresenter] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            _model = new BoardModel(_boardSize, _connectCount);
            _view = new BoardView(
                transform,
                _boardSize,
                _piecePrefab,
                _pieceMaterials,
                _columnSelectRoot,
                _deleteParticle,
                _pieceScaleFactor,
                ROTATION_DURATION
            );

            _deleteHandler = new BoardDeleteHandler(_model, _view);
            _dropHandler = new BoardDropHandler(_model, _view);
            _rotationUseCase = new BoardRotationUseCase(_model, _boardSize);
            _repositionUseCase = new BoardRepositionUseCase(_model);
            _viewMoveHandler = new BoardViewMoveHandler(_view);

            // 回転対象外のため親から分離
            _boardCollider.transform.SetParent(null);

            // 列選択オブジェクトの直下の子数を取得
            int childCount = _columnSelectRoot.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = _columnSelectRoot.transform.GetChild(i);

                // Plane タグならキャッシュ
                if (child.CompareTag(TAG_PLANE))
                {
                    _columnSelectPlane = child;
                    return;
                }
            }

            _currentPlayer = PLAYER_NONE;
        }

        public void OnUpdate(in float unscaledDeltaTime)
        {
            // 駒落下中、またはプレイヤー番号が不正なら非表示
            if (_isDrop || _currentPlayer == PLAYER_NONE)
            {
                _view.SeteColumnSelectVisible(false);
                return;
            }

            // Ray 生成
            Ray ray = _camera.ScreenPointToRay(_inputManager.Pointer);

            // ヒット判定
            int hitCount =
                Physics.RaycastNonAlloc(
                    ray,
                    _raycastHits,
                    Mathf.Infinity,
                    _raycastLayerMask);

            if (hitCount == 0)
            {
                _view.SeteColumnSelectVisible(false);
                return;
            }

            // ヒット対象取得
            RaycastHit hit = _raycastHits[0];

            // 対象外 Collider 判定
            if (hit.collider != _boardCollider)
            {
                _view.SeteColumnSelectVisible(false);
                return;
            }

            // --------------------------------------------------
            // 表示更新
            // --------------------------------------------------
            _view.SeteColumnSelectVisible(true);
            _view.UpdateColumnSelectPosition(hit.point);
        }

        public void OnExit()
        {
            // 購読解除
            _disposables.Dispose();
            _model.Dispose();

            UnbindPlayerChangeStream();
            UnbindInputStream();
        }

        public void OnPhaseEnter(in PhaseType phase)
        {
            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            if (_disposables.Count != 0)
            {
                return;
            }
            
            // ライン成立
            _model.OnLineComplete
                .Subscribe(async lineEvent =>
                {
                    _onLineComplete.OnNext(lineEvent);

                    await HandleLineDeleteAsync(lineEvent);
                })
                .AddTo(_disposables);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 駒配置入力ストリームを購読する
        /// </summary>
        /// <param name="dropStream">駒配置入力ストリーム</param>
        public void BindDropInputStream(in IObservable<Unit> dropStream)
        {
            // 既存購読があれば破棄
            _inputDisposables?.Dispose();

            // CompositeDisposable 初期化
            _inputDisposables = new CompositeDisposable();

            // 駒配置入力購読
            dropStream
                .Subscribe(_ =>
                {
                    // ドロップ中、またはプレイヤー未設定なら無効
                    if (_isDrop || _currentPlayer == PLAYER_NONE)
                    {
                        return;
                    }

                    // 駒配置処理
                    HandleDropColumn();
                })
                .AddTo(_inputDisposables);

            // 駒落下フラグ更新
            _isDrop = false;
        }

        /// <summary>
        /// 回転入力ストリームを購読する
        /// </summary>
        /// <param name="rotateStream">回転入力ストリーム</param>
        public void BindRotateInputStream(in IObservable<RotationCommand> rotateStream)
        {
            // 既存購読があれば破棄
            _inputDisposables?.Dispose();

            // CompositeDisposable 初期化
            _inputDisposables = new CompositeDisposable();

            // 回転入力購読
            rotateStream
                .Subscribe(cmd =>
                {
                    // 駒落下中、または回転中なら無効
                    if (_isDrop || _isRotate)
                    {
                        return;
                    }

                    // 非同期回転処理実行
                    HandleRotateAsync(cmd.Axis, cmd.Direction).Forget();
                })
                .AddTo(_inputDisposables);

            // 入力可能フラグ更新
            _isRotate = false;
        }
        
        /// <summary>
        /// 入力ストリームの購読を解除する
        /// </summary>
        public void UnbindInputStream()
        {
            _inputDisposables?.Dispose();
            _inputDisposables = null;
            
            _currentPlayer = PLAYER_NONE;

            // 列選択表示を非表示にする
            _view.SeteColumnSelectVisible(false);
        }

        /// <summary>
        /// プレイヤー変更ストリームを購読し、現在のプレイヤーインデックスを更新する
        /// </summary>
        /// <param name="player">プレイヤーインデックスを通知するストリーム</param>
        public void BindPlayerChangeStream(in IObservable<int> player)
        {
            // 多重購読防止
            _playerIndexSubscription?.Dispose();

            _playerIndexSubscription = player
                .Subscribe(player =>
                {
                    _currentPlayer = player;
                });
        }

        /// <summary>
        /// プレイヤー変更ストリームの購読を解除する
        /// </summary>
        public void UnbindPlayerChangeStream()
        {
            _playerIndexSubscription?.Dispose();
            _playerIndexSubscription = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// クリック座標から駒落下処理を開始
        /// </summary>
        private void HandleDropColumn()
        {
            // 列未選択状態なら処理なし
            if (!_view.IsColumnSelectVisible)
            {
                return;
            }

            // 列選択表示座標を列インデックスに変換
            _view.WorldToColumn(
                _columnSelectPlane.position.x,
                _columnSelectPlane.position.z,
                out int x,
                out int z);

            // 駒落下処理
            HandleDropAsync(x, z).Forget();
        }

        /// <summary>
        /// 駒落下処理
        /// </summary>
        private async UniTask HandleDropAsync(int x, int z)
        {
            // 配置可能判定
            int y = _model.CalculatePlace(x, z);

            if (y < 0)
            {
                return;
            }

            // 入力通知
            _onInputReceived.OnNext(Unit.Default);

            _isDrop = true;

            // ユースケース実行
            await _dropHandler.HandleDropAsync(x, y, z, _currentPlayer);

            // ライン成立チェック
            await CheckLine(_model.CheckLine());
        }

        /// <summary>
        /// 盤面回転処理
        /// </summary>
        private async UniTask HandleRotateAsync(
            RotationAxis axis,
            RotationDirection direction)
        {
            // 入力通知
            _onInputReceived.OnNext(Unit.Default);

            _isRotate = true;

            // --------------------------------------------------
            // 回転ユースケース実行
            // --------------------------------------------------
            RotationResult result = await _rotationUseCase.HandleRotateAsync(axis, direction);

            // ビュー辞書更新
            ApplyViewMoves(result.RotateMoves);

            // --------------------------------------------------
            // ビュー更新
            // --------------------------------------------------
            await _view.RotateAsync(
                transform,
                axis,
                direction
            );

            // 再配置アニメーション
            await _view.MovePiecesAsync(result.RepositionMoves);

            // 再配置後の位置をViewに確定反映
            ApplyViewMoves(result.RepositionMoves);

            // ライン成立チェック
            await CheckLine(_model.CheckLine());
        }

        /// <summary>
        /// ライン成立時に駒を削除し再配置を行う
        /// </summary>
        private async UniTask HandleLineDeleteAsync(LineCompleteEvent lineEvent)
        {
            LineDeleteResult result = 
                await _deleteHandler.HandleLineDeleteAsync(lineEvent);

            // --------------------------------------------------
            // 再配置処理
            //// --------------------------------------------------
            // 演出待機
            await UniTask.Delay(PIECE_DROP_DELAY_MS);

            await PiecesRepositionAsync(result.RepositionColumns);

            // ライン成立チェック
            await CheckLine(_model.CheckLine());
        }

        /// <summary>
        /// 駒再配置処理
        /// </summary>
        private async UniTask PiecesRepositionAsync(IReadOnlyList<(int x, int z)> pieces)
        {
            // --------------------------------------------------
            // ユースケース実行
            // --------------------------------------------------
            BoardRepositionResult result =
                await _repositionUseCase.HandleRepositionAsync(pieces);

            // 移動がない場合は処理なし
            if (result.Moves.Count == 0)
            {
                return;
            }

            // --------------------------------------------------
            // ビューアニメーション
            // --------------------------------------------------
            await _view.MovePiecesAsync(result.Moves);

            // --------------------------------------------------
            // ビュー辞書更新
            // --------------------------------------------------
            ApplyViewMoves(result.Moves);
        }

        /// <summary>
        /// ライン成立判定時の共通処理
        /// </summary>
        private async UniTask CheckLine(bool isLine)
        {
            // ラインが成立している場合は処理なし
            if (isLine)
            {
                return;
            }

            // 演出待機
            await UniTask.Delay(FINISH_LINE_CHECK_DELAY_MS);

            // フェーズ終了通知
            _onPlayerEnd.OnNext(Unit.Default);
        }

        /// <summary>
        /// ビューの駒辞書を移動情報に基づいて更新
        /// </summary>
        private void ApplyViewMoves(in IReadOnlyList<(BoardIndex from, BoardIndex to)> moves)
        {
            _viewMoveHandler.ApplyViewMoves(moves);
        }
    }
}