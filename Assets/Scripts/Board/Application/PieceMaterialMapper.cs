// ======================================================
// PieceMaterialMapper.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-10
// 更新日時 : 2026-04-10
// 概要     : プレイヤーIDに応じたマテリアル適用サービス
// ======================================================

using UnityEngine;

namespace BoardSystem.Application
{
    /// <summary>
    /// 駒マテリアル適用サービス
    /// </summary>
    public sealed class PieceMaterialMapper
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>プレイヤー別マテリアル配列</summary>
        private readonly Material[] _materials;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        public PieceMaterialMapper(Material[] materials)
        {
            _materials = materials;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// マテリアル適用
        /// </summary>
        public void Apply(in Renderer renderer, int playerId)
        {
            int index = playerId - 1;

            if (index < 0 || index >= _materials.Length)
            {
                return;
            }

            renderer.sharedMaterial = _materials[index];
        }
    }
}