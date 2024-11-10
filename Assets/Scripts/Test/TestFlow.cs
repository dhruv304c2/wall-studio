using UnityEngine;
using Zenject;
using CreateMode.Surface;
using UniRx;
using MonsterLove.StateMachine;
using ContextMenu = Context.ContextMenu;

namespace Test{
public class TestFlow : MonoBehaviour {
    public enum State{
        Waiting,
        WaitingSurface
    }

    [Inject] ISurfaceBuilder _surfaceBuilder;
    [Inject] ContextMenu.Pool _menuPool;
    StateMachine<State> _fsm;

    ContextMenu CreateWaitingMenu(){
        var menu = _menuPool.Spawn(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
        menu.AddKeyMap(OVRInput.Button.One, "Create Surface");
        return menu;
    }

    public void Start(){
        _fsm = new StateMachine<State>(this);
        _fsm.ChangeState(State.Waiting);
    }

    void Waiting_Enter(){
        var waitingObserver = Observable.EveryUpdate().TakeWhile(_ => _fsm.State == State.Waiting);
        var menu = CreateWaitingMenu();

        waitingObserver.Subscribe(_ => menu.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));

        waitingObserver
            .Where(_ => OVRInput.GetDown(OVRInput.Button.One))

            .Subscribe(_ => {
                _fsm.ChangeState(State.WaitingSurface);
                menu.Dispose();
            });
    }

    async void WaitingSurface_Enter(){
        await _surfaceBuilder.RequestVerticalSurfaceFromUserAsync(100);
        _fsm.ChangeState(State.Waiting);
    }
}
}

