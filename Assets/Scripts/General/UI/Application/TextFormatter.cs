// ======================================================
// TextFormatter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-09
// 更新日時 : 2026-04-09
// 概要     : 数値フォーマットを行い char 配列を生成するクラス
// ======================================================

namespace UISystem.Application
{
    /// <summary>
    /// 数値フォーマットクラス
    /// </summary>
    public sealed class TextFormatter
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>表示用文字バッファ</summary>
        private char[] _buffer;

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
        /// TextFormatter を生成
        /// </summary>
        /// <param name="format">フォーマット文字列</param>
        /// <param name="digits">各数値の表示桁数</param>
        public TextFormatter(
            in string format,
            in int[] digits)
        {
            _digits = digits;

            // フォーマット文字列長を取得
            int length = format.Length;

            // プレースホルダごとの数値開始位置配列を生成
            _numberStartIndexes = new int[digits.Length];

            // 一時バッファ生成
            // フォーマット + 数値桁数
            char[] temp = new char[length + TotalDigits(digits)];

            int writeIndex = 0;

            for (int i = 0; i < length; i++)
            {
                // プレースホルダ開始検出
                if (format[i] == PLACEHOLDER_BEGIN)
                {
                    int numberStart = i + 1;
                    int elementIndex = 0;

                    // プレースホルダ番号解析
                    while (format[numberStart] != PLACEHOLDER_END)
                    {
                        elementIndex =
                            elementIndex * 10 +
                            (format[numberStart] - ASCII_ZERO);

                        numberStart++;
                    }

                    int digit = digits[elementIndex];

                    // 書き込み開始位置記録
                    _numberStartIndexes[elementIndex] = writeIndex;

                    // 桁数分ゼロ埋め
                    for (int d = 0; d < digit; d++)
                    {
                        temp[writeIndex++] = ASCII_ZERO;
                    }

                    i = numberStart;
                }
                else
                {
                    temp[writeIndex++] = format[i];
                }
            }

            // 最終バッファ生成
            _buffer = new char[writeIndex];

            for (int i = 0; i < writeIndex; i++)
            {
                _buffer[i] = temp[i];
            }

            _placeholderCount = digits.Length;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 単一数値フォーマット
        /// 不足桁を 0 で埋める
        /// </summary>
        /// <param name="value">数値</param>
        /// <returns>フォーマット済み文字配列</returns>
        public char[] FormatWithPadding(in int value)
        {
            // 要素 0 に書き込み
            WriteDigitsWithPadding(value, 0);

            return _buffer;
        }

        /// <summary>
        /// 複数数値フォーマット
        /// 不足桁を 0 で埋める
        /// </summary>
        /// <param name="values">数値配列</param>
        /// <returns>フォーマット済み文字配列</returns>
        public char[] FormatWithPadding(in int[] values)
        {
            // 各プレースホルダへ書き込み
            for (int i = 0; i < _placeholderCount; i++)
            {
                WriteDigitsWithPadding(values[i], i);
            }

            return _buffer;
        }
        
        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 桁数合計取得
        /// </summary>
        private int TotalDigits(int[] digits)
        {
            int total = 0;

            for (int i = 0; i < digits.Length; i++)
            {
                total += digits[i];
            }

            return total;
        }

        /// <summary>
        /// 数値を書き込む
        /// 不足桁を 0 で埋める
        /// </summary>
        /// <param name="value">書き込み数値</param>
        /// <param name="elementIndex">プレースホルダ番号</param>
        private void WriteDigitsWithPadding(
            in int value,
            in int elementIndex)
        {
            // 表示桁数取得
            int digits = _digits[elementIndex];

            // 書き込み位置取得
            int index = _numberStartIndexes[elementIndex] + digits - 1;

            // 書き込み対象数値
            int number = value;

            for (int i = 0; i < digits; i++)
            {
                // 下位桁取得
                int digit = number % 10;

                // 数値文字を書き込み
                _buffer[index] = (char)(ASCII_ZERO + digit);

                // 次桁へ更新
                number /= 10;

                // 書き込み位置更新
                index--;
            }
        }
    }
}