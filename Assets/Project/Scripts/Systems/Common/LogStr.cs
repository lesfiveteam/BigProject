using System;

namespace BigProject.Systems
{
    public static class LogStr
    {
        public const string INFO_SESSION_STARTED = "=== Session started ===";
        public const string INFO_APPLICATION_QUITTING = "=== Application quitting ===";
        public const string INFO_DELETE_OLD_LOG_FILE = "Delete old log file: {0}.";
        public const string INFO_SCENE_LOADING = "Load scene: {0}.";
        public const string INFO_GAME_EXECUTION_MOVE = "Game execution move to stage: {0}.";
        public const string INFO_INITIALIZING_GAMEPLAY_SERVICES = "Start initializing gameplay services...";
        public const string INFO_INITIALIZING_GAMEPLAY_SERVICES_COMPLETED = "Gameplay services initialized.";
        public const string INFO_INITIALIZING_HUD = "Start initializing HUD widgets...";
        public const string INFO_INITIALIZING_HUD_COMPLETED = "HUD widgets initialized.";
        public const string INFO_INITIALIZING_SCENE_SERVICES = "Start initializing scene services...";
        public const string INFO_INITIALIZING_SCENE_SERVICES_COMPLETED = "Scene services initialized.";
        public const string INFO_REMOVING_GAMEPLAY_SERVICES = "Removing gameplay services...";
        public const string INFO_QUEST = "Quest progress: {0}.";

        public const string WARNING_UNHANDLED_SYSTEM_MESSAGE_TYPE = "Unhandled sysytem message type!\nMessage:\n{0}.";
        public const string WARNING_SAME_SCENE = "You are trying to load already loaded scene.";
        public const string WARNING_GAME_EXECUTION_INCORRECT_STAGE = "Game execution try move to incorrect stage: {0}.";
        public const string WARNING_GAME_EXECUTION_REWRITE_STAGE = "Game execution already at stage: {0}.";
        public const string WARNING_DUPLICATE_UNIQUE_ENTITY = "Try duplicate [{0}]. It should exist in one copy.";
        public const string WARNING_QUEST = "Quest warning: {0}.";

        public const string ERROR_WRITE_FAILED = "Logger write failed: {0}.";
        public const string ERROR_FILE_DELETE_FAILED = "Failed to delete {0}: {1}.";
        public const string ERROR_CREATE_DIRECTORY = "Cannot create log dir: {0}." +
            "\n\n Will be created in Persistent Data Path: " +
            "\n\t %userprofile%\\AppData\\LocalLow\\{1}\\{2}\\";
        public const string ERROR_QUEST = "Quest error: {0}.";

        public const string CRITICAL_UNABLE_GET_SERVICE = "{0}: can't get {1} service.";
        public const string CRITICAL_NOT_SERIALIZED_FIELD = "{0}: has not serialized field [{1}].";
        public const string CRITICAL_NULL_REFERENCE = "{0}: get null reference [{1}].";
    }
}