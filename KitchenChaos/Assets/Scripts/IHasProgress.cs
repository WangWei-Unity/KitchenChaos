using System;
using UnityEngine;

public interface IHasProgress
{
    //处理进度条变化的事件
    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;
    public class OnProgressChangedEventArgs : EventArgs
    {
        //进度归一处理
        public float progressNormalized;
    }
}
