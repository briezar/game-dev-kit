using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IInitializable { void Init(); }
public interface IInitializable<T1> { void Init(T1 arg1); }
public interface IInitializable<T1, T2> { void Init(T1 arg1, T2 arg2); }

public interface IAsyncInitializable { UniTask Init(); }
public interface IAsyncInitializable<T1> { UniTask Init(T1 arg1); }
public interface IAsyncInitializable<T1, T2> { UniTask Init(T1 arg1, T2 arg2); }