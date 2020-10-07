#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using QML;
using QmlFiles.scene;
using QtQuick;

namespace OpenTemple.Core.Ui
{
    public class UiSceneDefinition
    {
        public string Id { get; }

        public string QmlPath { get; }

        public UiSceneDefinition(string id, string qmlPath)
        {
            Id = id;
            QmlPath = qmlPath;
        }
    }

    /// <summary>
    /// Manages all available scenes.
    /// </summary>
    public class UiSceneDefinitionManager
    {
        private readonly Dictionary<string, UiSceneDefinition> _scenes = new Dictionary<string, UiSceneDefinition>();

        /// <summary>
        /// Adds a new scene definition.
        /// </summary>
        public void AddSceneDefinition(UiSceneDefinition sceneDefinition)
        {
            if (!_scenes.TryAdd(sceneDefinition.Id, sceneDefinition))
            {
                throw new ArgumentException($"Scene definition id {sceneDefinition.Id} is already in use.");
            }
        }

        public UiSceneDefinition Get(string id)
        {
            return _scenes[id];
        }
    }

    public class SceneCreatedEvent
    {
        public string SceneId => SceneDefinition.Id;

        public UiSceneDefinition SceneDefinition { get; }

        public QQuickItem SceneItem { get; }

        public SceneCreatedEvent(UiSceneDefinition sceneDefinition, QQuickItem sceneItem)
        {
            SceneDefinition = sceneDefinition;
            SceneItem = sceneItem;
        }
    }

    /// <summary>
    /// Manages the currently visible scene and allows switching between scenes.
    /// </summary>
    public class UiSceneManager
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly IUserInterface _ui;

        private readonly UiSceneDefinitionManager _definitionManager;

        private readonly Task<RootSceneQml> _rootItem;

        public event Action<SceneCreatedEvent>? OnSceneCreated;

        public UiSceneManager(IUserInterface ui, UiSceneDefinitionManager definitionManager)
        {
            _ui = ui;
            _definitionManager = definitionManager;

            _rootItem = _ui.LoadView<RootSceneQml>("scene/RootScene.qml");
        }

        public async Task Show(string id)
        {
            Logger.Info("Switching to scene {0}", id);

            var sceneDefinition = _definitionManager.Get(id);

            var rootItem = await _rootItem;

            var sceneItem = await _ui.LoadView<QQuickItem>(sceneDefinition.QmlPath);
            await _ui.PostTask(() =>
            {
                sceneItem.IsJavaScriptOwned = true;
                OnSceneCreated?.Invoke(new SceneCreatedEvent(sceneDefinition, sceneItem));
                rootItem.ShowItem(sceneItem);
            });
        }

        // if the given scene is the active one, hide it
        public async Task Pop(string sceneId)
        {
            var rootItem = await _rootItem;
            await _ui.PostTask(() =>
            {
                rootItem.Clear();
            });
        }
    }
}