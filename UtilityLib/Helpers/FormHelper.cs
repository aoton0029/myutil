using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Helpers
{
    public static class FormHelper
    {
        public static void CenterOnParent(this Form form)
        {
            if (form.Owner == null) return;
            form.Location = new Point(
                x: form.Owner.Location.X + form.Owner.Width / 2 - form.Width / 2,
                y: form.Owner.Location.Y + form.Owner.Height / 2 - form.Height / 2);
        }


    }
}
