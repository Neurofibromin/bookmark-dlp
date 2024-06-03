using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookmark_dlp.Models
{
    public class MessageBus
    {
        public static event EventHandler<string> ButtonClicked;

        public static void RaiseButtonClicked(string buttonText)
        {
            ButtonClicked?.Invoke(null, buttonText);
        }
    }
    
    public class MessageBus2
    {
        public static event EventHandler<string> ButtonClicked;

        public static void RaiseButtonClicked(string buttonText)
        {
            ButtonClicked?.Invoke(null, buttonText);
        }
    }
}
