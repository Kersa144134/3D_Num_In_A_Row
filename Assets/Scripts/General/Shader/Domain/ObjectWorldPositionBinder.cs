// ======================================================
// ObjectWorldPositionBinder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-18
// 更新日時 : 2026-05-18
// 概要     : オブジェクトのワールド座標をシェーダーへ送信するバインダー
// ======================================================

using UnityEngine;

namespace ShaderSystem.Domain
{
    /// <summary>
    /// オブジェクトのワールド座標をマテリアルに渡すコンポーネント
    /// Shader Graph側で _ObjectPosition を利用可能にする
    /// </summary>
    [ExecuteAlways]
    public sealed class ObjectWorldPositionBinder : MonoBehaviour
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>位置情報対象 Renderer</summary>
        [SerializeField]
        private Renderer _targetRenderer;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// Rendererごとの MaterialPropertyBlock
        /// </summary>
        private MaterialPropertyBlock _propertyBlock;

        // ======================================================
        // シェーダープロパティ定義
        // ======================================================

        /// <summary>オブジェクト座標</summary>
        private static readonly int OBJECT_POSITION_ID = Shader.PropertyToID("_ObjectPosition");

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponent<Renderer>();
            }

            _propertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            Vector3 worldPosition = transform.position;

            // 現在の MaterialPropertyBlock を取得
            _targetRenderer.GetPropertyBlock(_propertyBlock);

            // シェーダーへ値を設定
            _propertyBlock.SetVector(OBJECT_POSITION_ID, worldPosition);

            // Renderer へ反映
            _targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}