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

        /// <summary>空白文字</summary>
        private const char SPACE = ' ';

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

        /// <summary>
        /// 単一数値フォーマット
        /// 左詰めで書き込み、余りを空白で埋める
        /// </summary>
        /// <param name="value">数値</param>
        /// <returns>フォーマット済み文字配列</returns>
        public char[] FormatWithSpacePadding(in int value)
        {
            // 要素 0 に書き込み
            WriteDigitsWithSpacePadding(value, 0);

            return _buffer;
        }

        /// <summary>
        /// 複数数値フォーマット
        /// 左詰めで書き込み、余りを空白で埋める
        /// </summary>
        /// <param name="values">数値配列</param>
        /// <returns>フォーマット済み文字配列</returns>
        public char[] FormatWithSpacePadding(in int[] values)
        {
            // 各プレースホルダへ書き込み
            for (int i = 0; i < _placeholderCount; i++)
            {
                WriteDigitsWithSpacePadding(values[i], i);
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

        /// <summary>
        /// 数値を左詰めで書き込む
        /// 余った領域は空白で埋める
        /// </summary>
        /// <param name="value">書き込み数値</param>
        /// <param name="elementIndex">プレースホルダ番号</param>
        private void WriteDigitsWithSpacePadding(
            in int value,
            in int elementIndex)
        {
            // 表示桁数取得
            int digits = _digits[elementIndex];

            // 書き込み開始位置取得
            int startIndex = _numberStartIndexes[elementIndex];

            // 数値桁数取得
            int numberDigits = CountDigits(value);

            // 表示桁数を超える場合は右側を使用
            if (numberDigits > digits)
            {
                numberDigits = digits;
            }

            // 数値を一時格納
            int number = value;

            // 数値文字を逆順で格納
            char[] temp = new char[numberDigits];

            for (int i = numberDigits - 1; i >= 0; i--)
            {
                temp[i] = (char)(ASCII_ZERO + (number % 10));

                number /= 10;
            }

            // 左詰めで書き込み
            for (int i = 0; i < numberDigits; i++)
            {
                _buffer[startIndex + i] = temp[i];
            }

            // 余りを空白で埋める
            for (int i = numberDigits; i < digits; i++)
            {
                _buffer[startIndex + i] = SPACE;
            }
        }

        /// <summary>
        /// 数値の桁数取得
        /// </summary>
        /// <param name="value">対象数値</param>
        /// <returns>桁数</returns>
        private int CountDigits(in int value)
        {
            // 0 は 1 桁として扱う
            if (value == 0)
            {
                return 1;
            }

            // 算出した桁数
            int count = 0;

            // 桁数計算用の作業変数
            int number = value;

            // 数値が 0 になるまで繰り返す
            while (number > 0)
            {
                // 下位 1 桁を除去する
                number /= 10;

                // 除去した桁数を加算する
                count++;
            }

            // 算出した桁数を返す
            return count;
        }
    }
}