// ======================================================
// TextFormatService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-09
// 更新日時 : 2026-03-09
// 概要     : TextMeshProへGCを発生させず数値フォーマットを行う
// ======================================================

using TMPro;

namespace UISystem.Service
{
    /// <summary>
    /// テキストフォーマットサービス
    /// </summary>
    public sealed class TextFormatService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>出力先テキスト</summary>
        private readonly TextMeshProUGUI _text;

        /// <summary>表示用文字バッファ</summary>
        private readonly char[] _buffer;

        /// <summary>プレースホルダ数</summary>
        private readonly int _placeholderCount;

        /// <summary>各プレースホルダの数値書き込み開始位置</summary>
        private readonly int[] _numberStartIndexes;

        /// <summary>各プレースホルダの表示桁数</summary>
        private readonly int[] _digits;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>プレースホルダ開始文字</summary>
        private const char PLACEHOLDER_BEGIN = '{';

        /// <summary>プレースホルダ終了文字</summary>
        private const char PLACEHOLDER_END = '}';

        /// <summary>数値文字の基準 ASCII コード</summary>
        private const char ASCII_ZERO = '0';

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// TextFormatService を生成
        /// </summary>
        /// <param name="text">出力先テキスト</param>
        /// <param name="format">フォーマット文字列</param>
        /// <param name="digits">各数値の表示桁数</param>
        public TextFormatService(
            in TextMeshProUGUI text,
            in string format,
            in int[] digits)
        {
            _text = text;
            _digits = digits;

            // フォーマット文字列長を取得
            int length = format.Length;

            // プレースホルダごとの数値開始位置配列を生成
            _numberStartIndexes = new int[digits.Length];

            // --------------------------------------------------
            // 一時バッファ生成
            // --------------------------------------------------
            // フォーマット文字数 + 数値桁数合計
            char[] temp = new char[length + TotalDigits(digits)];

            int writeIndex = 0;

            for (int i = 0; i < length; i++)
            {
                // プレースホルダ開始文字検出
                if (format[i] == PLACEHOLDER_BEGIN)
                {
                    // プレースホルダ内の要素番号読み取り開始位置
                    int numberStart = i + 1;

                    // プレースホルダの要素番号
                    int elementIndex = 0;

                    // プレースホルダ終了文字に到達するまで数値を読み取る
                    while (format[numberStart] != PLACEHOLDER_END)
                    {
                        // 数値を左シフトで 1 桁ずつ読み取り整数へ変換
                        elementIndex = elementIndex * 10 + (format[numberStart] - ASCII_ZERO);

                        // 次の文字へ移動
                        numberStart++;
                    }

                    // このプレースホルダの表示桁数を取得
                    int digit = digits[elementIndex];

                    // このプレースホルダの数値書き込み開始位置をキャッシュ
                    _numberStartIndexes[elementIndex] = writeIndex;

                    // 表示桁数分のゼロ文字をバッファへ挿入
                    for (int d = 0; d < digit; d++)
                    {
                        temp[writeIndex++] = ASCII_ZERO;
                    }

                    // プレースホルダ終端の位置までスキップ
                    i = numberStart;
                }
                else
                {
                    // そのままバッファへ書き込む
                    temp[writeIndex++] = format[i];
                }
            }

            // --------------------------------------------------
            // 最終バッファ生成
            // --------------------------------------------------
            _buffer = new char[writeIndex];

            // 一時バッファ内容をコピー
            for (int i = 0; i < writeIndex; i++)
            {
                _buffer[i] = temp[i];
            }

            // プレースホルダ数を保存
            _placeholderCount = digits.Length;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 単一数値更新
        /// </summary>
        /// <param name="value">表示する数値</param>
        public void SetNumberText(in int value)
        {
            // 要素0へ数値を書き込み
            WriteDigits(value, 0);

            // TextMeshProへ文字配列を直接送信
            _text.SetCharArray(_buffer);
        }

        /// <summary>
        /// 配列数値更新
        /// </summary>
        /// <param name="values">表示する数値配列</param>
        public void SetNumberText(in int[] values)
        {
            // 各プレースホルダへ数値を書き込み
            for (int i = 0; i < _placeholderCount; i++)
            {
                WriteDigits(values[i], i);
            }

            // TextMeshProへ文字配列を直接送信
            _text.SetCharArray(_buffer);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 数値を書き込む
        /// </summary>
        /// <param name="value">書き込む数値</param>
        /// <param name="elementIndex">プレースホルダ番号</param>
        private void WriteDigits(in int value, in int elementIndex)
        {
            // 指定プレースホルダの表示桁数取得
            int digits = _digits[elementIndex];

            // 書き込み開始位置計算
            int index = _numberStartIndexes[elementIndex] + digits - 1;

            // 計算用数値コピー
            int number = value;

            // 指定桁数分ループ
            for (int i = 0; i < digits; i++)
            {
                // 下位桁取得
                int digit = number % 10;

                // ASCII数値へ変換して書き込み
                _buffer[index] = (char)(ASCII_ZERO + digit);

                // 次桁計算
                number /= 10;

                // 書き込み位置を左へ移動
                index--;
            }
        }

        /// <summary>
        /// 表示桁数合計取得
        /// </summary>
        /// <param name="digits">桁数配列</param>
        /// <returns>桁数合計</returns>
        private static int TotalDigits(int[] digits)
        {
            // 合計値
            int total = 0;

            // 各桁数を加算
            for (int i = 0; i < digits.Length; i++)
            {
                total += digits[i];
            }

            // 合計返却
            return total;
        }
    }
}