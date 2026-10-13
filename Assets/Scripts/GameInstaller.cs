using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private TurretPlacementController _turretPlacementController;
    [SerializeField] private JourneyManager _journeyManager;
    [SerializeField] private CurrencyManager _currencyManager;
    [SerializeField] private RailLoopController _railLoopController;
    [SerializeField] private TrainEngine _trainEngine;
    [SerializeField] private RewardedAdManager _rewardedAdManager;

    [Header("UI Bindings")]
    [SerializeField] private CanvasGroup _mainCanvasGroup;
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private GameOverPanel _gameOverPanel;

    public override void InstallBindings()
    {
        Container.Bind<WagonUpgradeUI>()
           .FromComponentInHierarchy(includeInactive: true)
           .AsSingle();
        Container.Bind<SaveManager>().AsSingle();
        Container.Bind<CreateTurretUI>()
                 .FromComponentInHierarchy(includeInactive: true)
                 .AsSingle();

        Container.Bind<TrainShopUIController>()
                 .FromComponentInHierarchy(includeInactive: true)
                 .AsSingle();
        Container.Bind<TrainBuilder>().FromComponentInHierarchy().AsSingle();
        Container.Bind<RewardedAdManager>().FromInstance(_rewardedAdManager).AsSingle();
        Container.Bind<CanvasGroup>().FromInstance(_mainCanvasGroup).AsSingle();
        Container.Bind<Canvas>().FromInstance(_mainCanvas).AsSingle();
        Container.Bind<TurretDemolitionController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<RailLoopController>().FromInstance(_railLoopController).AsSingle();
        Container.Bind<CurrencyManager>().FromInstance(_currencyManager).AsSingle();
        Container.Bind<JourneyManager>().FromInstance(_journeyManager).AsSingle();
        Container.Bind<Camera>().FromInstance(_mainCamera).AsSingle();
        Container.Bind<TurretPlacementController>().FromInstance(_turretPlacementController).AsSingle();
        Container.Bind<TrainCameraController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<DialogController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameOverPanel>().FromInstance(_gameOverPanel).AsSingle();
        Container.Bind<TrainEngine>().FromInstance(_trainEngine).AsSingle();
    }
}