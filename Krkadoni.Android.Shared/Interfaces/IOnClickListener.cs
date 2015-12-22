using Android.Views;

namespace Com.Krkadoni.Interfaces
{
    public interface IOnClickListener
    {
        void OnClick(View view, int position);

        void OnLongClick(View view, int position);
    }
}

