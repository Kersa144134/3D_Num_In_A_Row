// ======================================================
// BoardPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-07
// 概要     : 3D 目並べゲームのボードを制御するクラス
// ======================================================

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using BoardSystem.Application;
using BoardSystem.Domain;
using InputSystem.Presentation;
using OptionSystem.Presentation;
using SoundSystem.Domain;
using SoundSystem.Presentation;
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

        /// <summary>ボードの列選択表示ルート</summary>
        [SerializeField]
        private GameObject _columnSelectRoot;

        [Header("駒")]
        /// <summary>駒の Prefab</summary>
        [SerializeField]
        private GameObject _piecePrefab;

        /// <summary>駒のマテリアル配列</summary>
        [SerializeField]
        private Material[] _pieceMaterials;

        /// <summary>駒削除時に再生するパーティクル</summary>
        [SerializeField]
        private GameObject _deleteParticle;

        /// <summary>駒のスケール倍率</summary>
        [SerializeField, Range(0.5f, 1.0f)]
        private float _pieceScaleFactor;

        [Header("ヒット判定")]
        /// <summary>ボードのクリック判定用 GameObject</summary>
        [SerializeField]
        private GameObject _boardHit;

        /// <summary>Ray 判定用レイヤー</summary>
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

        /// <summary>ボード回転ユースケース</summary>
        private BoardRotationUseCase _rotationUseCase;

        /// <summary>駒再配置ユースケース</summary>
        private BoardRepositionUseCase _repositionUseCase;

        /// <summary>駒ビュー更新ハンドラ</summary>
        private BoardViewPieceMapUpdater _viewPieceMapUpdater;

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        /// <summary>SoundManager キャッシュ</summary>
        private SoundManager _soundManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>カメラ</summary>
        private Camera _camera;

        /// <summary>RaycastHit 配列</summary>
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        /// <summary>ボードの列選択表示プレーン</summary>
        private Transform _columnSelectPlane;

        /// <summary>ボードのクリック判定用 Collider</summary>
        private Collider _boardHitCollider;

        /// <summary>ボードのクリック判定用 Renderer</summary>
        private Renderer _boardHitRenderer;

        /// <summary>ボードサイズ</summary>
        private int _boardSize;

        /// <summary>ライン成立条件</summary>
        private int _connectCount;

        /// <summary>現在のプレイヤーID</summary>
        private int _currentPlayer = PLAYER_NONE;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>プレイヤー未指定 ID</summary>
        private const int PLAYER_NONE = -1;

        /// <summary>Planeタグ</summary>
        private const string TAG_PLANE = "Plane";

        /// <summary>ボードの回転アニメーション時間（秒）</summary>
        private const float ROTATION_DURATION = 0.5f;

        /// <summary>駒削除後、落下処理を開始するまでの待機時間（秒）</summary>
        private const float PIECE_DROP_DELAY = 0.5f;

        /// <summary>ライン成立終了後、プレイヤー切り替えを開始するまでの待機時間（秒）</summary>
        private const float FINISH_LINE_CHECK_DELAY = 0.75f;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>駒落下入力通知用 Subject</summary>
        private readonly Subject<Unit> _onDropInputted = new Subject<Unit>();

        /// <summary>駒落下入力ストリーム</summary>
        public IObservable<Unit> OnDropInputted => _onDropInputted;

        /// <summary>ボード回転入力通知用 Subject</summary>
        private readonly Subject<Unit> _onRotateInputted = new Subject<Unit>();

        /// <summary>ボード回転入力ストリーム</summary>
        public IObservable<Unit> OnRotateInputted => _onRotateInputted;

        /// <summary>ライン成立通知用 Subject</summary>
        private readonly Subject<IReadOnlyList<LineCompleteEvent>> _onLineComplete =
            new Subject<IReadOnlyList<LineCompleteEvent>>();

        /// <summary>ライン成立ストリーム</summary>
        public IObservable<IReadOnlyList<LineCompleteEvent>> OnLineComplete => _onLineComplete;

        /// <summary>ライン削除通知用 Subject</summary>
        private readonly Subject<Unit> _onLineDelete = new();

        /// <summary>ライン削除通知ストリーム</summary>
        public IObservable<Unit> OnLineDelete => _onLineDelete;

        /// <summary>ライン位置通知ストリーム</summary>
        public IObservable<LinePositionInfo> OnLinePositionNotified => _deleteHandler.OnLinePositionNotified;

        /// <summary>ライン発光実行ストリーム</summary>
        public IObservable<Unit> OnLineEmissionExecuted => _deleteHandler.OnLineEmissionExecuted;

        /// <summary>プレイヤー行動終了通知用 Subject</summary>
        private readonly Subject<Unit> _onPlayerEnd = new Subject<Unit>();

        /// <summary>プレイヤー行動終了ストリーム</summary>
        public IObservable<Unit> OnPlayerEnd => _onPlayerEnd;

        /// <summary>プレイヤーインデックス購読</summary>
        private IDisposable _playerIndexSubscription;

        /// <summary>ドロップ入力購読</summary>
        private IDisposable _dropInputSubscription;

        /// <summary>回転入力購読管理</summary>
        private IDisposable _rotateInputSubscription;

        /// <summary>列選択表示の表示状態</summary>
        public IReadOnlyReactiveProperty<bool> IsColumnSelectVisible => _view.IsColumnSelectVisible;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;
            _soundManager = SoundManager.Instance;

            // シーン内のメインカメラを取得
            _camera = Camera.main;

            if (_gameOptionManager == null || _inputManager == null || _camera == null ||
                _piecePrefab == null || _deleteParticle == null ||
                _boardHit == null || _columnSelectRoot == null)
            {
                Debug.LogError("[BoardPresenter] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            // オプション取得
            _boardSize = _gameOptionManager.BoardSize;
            _connectCount = _gameOptionManager.ConnectCount;

            // --------------------------------------------------
            // ボードクリック判定初期化
            // --------------------------------------------------
            // 回転対象外のため親から分離
            _boardHit.transform.SetParent(null);

            // Collider, Renderer 取得
            _boardHitCollider = _boardHit.GetComponent<Collider>();
            _boardHitRenderer = _boardHit.GetComponent<Renderer>();

            // --------------------------------------------------
            // モデル、ビュー初期化
            // --------------------------------------------------
            _model = new BoardModel(_boardSize, _connectCount);
            _view = new BoardView(
                transform,
                _boardSize,
                _piecePrefab,
                _pieceMaterials,
                _boardHitRenderer,
                _columnSelectRoot,
                _deleteParticle,
                _pieceScaleFactor,
                ROTATION_DURATION
            );

            // --------------------------------------------------
            // クラス初期化
            // --------------------------------------------------
            _deleteHandler = new BoardDeleteHandler(_model, _view);
            _rotationUseCase = new BoardRotationUseCase(_model, _boardSize);
            _repositionUseCase = new BoardRepositionUseCase(_model);
            _viewPieceMapUpdater = new BoardViewPieceMapUpdater(_view);

            // --------------------------------------------------
            // 列選択表示初期化
            // --------------------------------------------------
            // 列選択オブジェクトの直下の子数を取得
            int childCount = _columnSelectRoot.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = _columnSelectRoot.transform.GetChild(i);

                if (child.CompareTag(TAG_PLANE))
                {
                    _columnSelectPlane = child;

                    break;
                }
            }

            // イベント購読
            Subscribe();
        }

        public void OnUpdate(in float unscaledDeltaTime)
        {
            // 駒配置入力が未購読、またはプレイヤー ID が不正の場合
            if (_dropInputSubscription == null || _currentPlayer == PLAYER_NONE)
            {
                _view.SetBoardHitVisible(false);
                _view.SetColumnSelectVisible(false);

                return;
            }
            else
            {
                _view.SetBoardHitVisible(true);
            }

            // Ray 生成
            Ray ray = _camera.ScreenPointToRay(_inputManager.Pointer);

            // ヒット判定
            int hitCount = Physics.RaycastNonAlloc(
                ray,
                _raycastHits,
                Mathf.Infinity,
                _raycastLayerMask);

            if (hitCount == 0)
            {
                _view.SetColumnSelectVisible(false);

                return;
            }

            // ヒット対象取得
            RaycastHit hit = _raycastHits[0];

            // 対象外 Collider 判定
            if (hit.collider != _boardHitCollider)
            {
                _view.SetColumnSelectVisible(false);

                return;
            }

            // --------------------------------------------------
            // 表示更新
            // --------------------------------------------------
            _view.SetColumnSelectVisible(true);
            _view.UpdateColumnSelectPosition(hit.point);
        }

        public void OnExit()
        {
            // 購読解除
            _disposables?.Dispose();
            _model?.Dispose();
            _deleteHandler?.Dispose();

            UnbindPlayerChangeStream();
            UnbindDropInputStream();
            UnbindRotateInputStream();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// プレイヤー変更ストリームを購読する
        /// </summary>
        /// <param name="player">プレイヤー ID 通知ストリーム</param>
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
        /// 駒配置入力ストリームを購読する
        /// </summary>
        /// <param name="dropStream">駒配置入力ストリーム</param>
        public void BindDropInputStream(in IObservable<Unit> dropStream)
        {
            // 多重購読防止
            _dropInputSubscription?.Dispose();

            _dropInputSubscription = dropStream
                .Subscribe(_ =>
                {
                    // プレイヤー ID 未設定なら無効
                    if (_currentPlayer == PLAYER_NONE)
                    {
                        return;
                    }

                    // 駒配置処理
                    HandleDropColumn();
                });
        }

        /// <summary>
        /// 駒配置入力ストリームの購読を解除する
        /// </summary>
        public void UnbindDropInputStream()
        {
            _dropInputSubscription?.Dispose();
            _dropInputSubscription = null;
        }

        /// <summary>
        /// 回転入力ストリームを購読する
        /// </summary>
        /// <param name="rotateStream">回転入力ストリーム</param>
        public void BindRotateInputStream(in IObservable<RotationCommand> rotateStream)
        {
            // 多重購読防止
            _rotateInputSubscription?.Dispose();

            _rotateInputSubscription = new CompositeDisposable();

            _rotateInputSubscription = rotateStream
                .Subscribe(cmd =>
                {
                    // ボード回転処理
                    HandleRotateAsync(cmd.Axis, cmd.Direction).Forget();
                });
        }
        
        /// <summary>
        /// 入力ストリームの購読を解除する
        /// </summary>
        public void UnbindRotateInputStream()
        {
            _rotateInputSubscription?.Dispose();
            _rotateInputSubscription = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// イベント購読
        /// </summary>
        private void Subscribe()
        {
            // ライン成立
            _model.OnLineComplete
                .Subscribe(async lineEvent =>
                {
                    _onLineComplete.OnNext(lineEvent);

                    await HandleLineDeleteAsync(lineEvent);
                })
                .AddTo(_disposables);
            
            // --------------------------------------------------
            // SE 再生
            // --------------------------------------------------
            // 選択列更新
            _view.OnSelectColumnChanged
                .Subscribe(_ => _soundManager?.PlaySE(SeType.Board_ColumnSelect, 0.5f))
                .AddTo(_disposables);
            // ピース発光
            _deleteHandler.OnPieceEmissionExecuted
                .Subscribe(_ => _soundManager?.PlaySE(SeType.Piece_Emission, 0.75f))
                .AddTo(_disposables);
            // ライン削除
            _deleteHandler.OnLineDeleteExecuted
                .Subscribe(_ => _soundManager?.PlaySE(SeType.Piece_Delete, 0.75f))
                .AddTo(_disposables);
        }

        /// <summary>
        /// プレイヤー変更ストリームの購読を解除する
        /// </summary>
        private void UnbindPlayerChangeStream()
        {
            _playerIndexSubscription?.Dispose();
            _playerIndexSubscription = null;
        }

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// クリック座標から駒落下処理を開始
        /// </summary>
        private void HandleDropColumn()
        {
            // 列未選択状態なら処理なし
            if (!_view.IsColumnSelectVisible.Value)
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
            _onDropInputted.OnNext(Unit.Default);

            // 駒配置入力購読解除
            UnbindDropInputStream();

            // SE 再生
            _soundManager?.PlaySE(SeType.Piece_Drop, 0.75f);

            // 駒生成
            PieceData piece = await _view.SpawnPieceAsync(x, y, z, _currentPlayer);

            // インデックス生成
            BoardIndex index = new BoardIndex(x, y, z);

            // ビューに駒情報登録
            _view.SetPiece(index, piece);

            // モデルの盤面更新
            _model.ApplyPlace(index, _currentPlayer);

            // ライン成立チェック
            await CheckLine(_model.CheckLine());
        }

        /// <summary>
        /// ボード回転処理
        /// </summary>
        private async UniTask HandleRotateAsync(
            RotationAxis axis,
            RotationDirection direction)
        {
            // 入力通知
            _onRotateInputted.OnNext(Unit.Default);

            // ボード回転入力購読解除
            UnbindRotateInputStream();

            // SE 再生
            _soundManager?.PlaySE(SeType.Board_Rotate);

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
        private async UniTask HandleLineDeleteAsync(IReadOnlyList<LineCompleteEvent> lineEvents)
        {
            // 再配置対象となる列の算出
            LineDeleteResult deleteResult = await _deleteHandler.HandleLineDeleteAsync(lineEvents);
            
            // ライン削除実行通知
            _onLineDelete.OnNext(Unit.Default);

            // 演出待機
            await UniTask.Delay(TimeSpan.FromSeconds(PIECE_DROP_DELAY));

            // --------------------------------------------------
            // 再配置処理
            //// --------------------------------------------------
            BoardRepositionResult repositionResult =
                await _repositionUseCase.HandleRepositionAsync(deleteResult.RepositionColumns);

            // 移動がない場合は以降の処理なし
            if (repositionResult.Moves.Count == 0)
            {
                // 演出待機
                await UniTask.Delay(TimeSpan.FromSeconds(FINISH_LINE_CHECK_DELAY));

                // フェーズ終了通知
                _onPlayerEnd.OnNext(Unit.Default);

                return;
            }

            // --------------------------------------------------
            // ビューアニメーション
            // --------------------------------------------------
            await _view.MovePiecesAsync(repositionResult.Moves);

            // --------------------------------------------------
            // ビュー辞書更新
            // --------------------------------------------------
            ApplyViewMoves(repositionResult.Moves);

            // ライン成立チェック
            await CheckLine(_model.CheckLine());
        }

        // --------------------------------------------------
        // 共通
        // --------------------------------------------------
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
            await UniTask.Delay(TimeSpan.FromSeconds(FINISH_LINE_CHECK_DELAY));

            // フェーズ終了通知
            _onPlayerEnd.OnNext(Unit.Default);
        }

        /// <summary>
        /// ビューの駒辞書を移動情報に基づいて更新
        /// </summary>
        private void ApplyViewMoves(in IReadOnlyList<(BoardIndex from, BoardIndex to)> moves)
        {
            _viewPieceMapUpdater.UpdatePieceMap(moves);
        }
    }
}