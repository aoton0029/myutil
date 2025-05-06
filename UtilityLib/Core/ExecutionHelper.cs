using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    public class ExceptionHandler
    {
        private readonly LoggingService _logger;

        public ExceptionHandler(LoggingService logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private void OnThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex, e.IsTerminating);
            }
        }

        public void HandleException(Exception ex, bool isTerminating = false)
        {
            try
            {
                _logger.Error("アプリケーションエラーが発生しました", ex);

                string message = $"エラーが発生しました。\n\n{ex.Message}";
                if (isTerminating)
                {
                    message += "\n\nアプリケーションは終了します。";
                }

                MessageBox.Show(
                    message,
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // ロギングに失敗した場合のフォールバック処理
                MessageBox.Show(
                    "重大なエラーが発生しました。",
                    "システムエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public T ExecuteWithErrorHandling<T>(Func<T> action, T defaultValue = default)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return defaultValue;
            }
        }

        public void ExecuteWithErrorHandling(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
    }
}
