import json
import configparser
import os
import shutil
from pathlib import Path
from typing import Dict, Any, Optional
from .config_loader import config_loader


class ConfigManager:
    """
    Clase para gestionar archivos de configuración del servidor Project Zomboid
    """
    
    def __init__(self, server_path: str = ""):
        # Cargar configuración del servidor
        server_paths = config_loader.get_server_paths()
        if server_paths and not server_path:
            server_path = server_paths.get('server_path', '')
            
        self.server_path = Path(server_path) if server_path else Path.cwd()
        self.backup_dir = self.server_path / "config_backups"
        
        # Cargar archivos de configuración desde JSON
        current_server = config_loader.get_current_server_config()
        if current_server:
            self.config_files = current_server.get('config_files', {})
        else:
            self.config_files = {}
        
    def read_ini_file(self, file_path: str) -> Dict[str, Any]:
        """
        Lee un archivo INI y retorna su contenido como diccionario
        """
        try:
            # Intentar diferentes codificaciones para leer el archivo
            encodings = ['utf-8', 'latin-1', 'cp1252', 'iso-8859-1']
            content = None
            
            for encoding in encodings:
                try:
                    with open(file_path, 'r', encoding=encoding) as file:
                        content = file.read()
                    break
                except UnicodeDecodeError:
                    continue
            
            if content is None:
                print(f"Error reading INI file {file_path}: No se pudo decodificar con ninguna codificación")
                return {}
            
            # Verificar si el contenido tiene headers de sección
            has_sections = any(line.strip().startswith('[') and line.strip().endswith(']') 
                             for line in content.split('\n'))
            
            result = {}
            
            if has_sections:
                # Parsear como INI normal con secciones
                config = configparser.ConfigParser(allow_no_value=True)
                config.read_string(content)
                
                for section in config.sections():
                    result[section] = dict(config[section])
            else:
                # Tratar como archivo de configuración sin secciones (crear sección DEFAULT)
                lines = content.strip().split('\n')
                default_section = {}
                for line in lines:
                    line = line.strip()
                    if line and not line.startswith('#') and '=' in line:
                        key, value = line.split('=', 1)
                        default_section[key.strip()] = value.strip()
                if default_section:
                    result['DEFAULT'] = default_section
                
            return result
        except Exception as e:
            print(f"Error reading INI file {file_path}: {e}")
            return {}
            
    def write_ini_file(self, file_path: str, data: Dict[str, Any]) -> bool:
        """
        Escribe datos a un archivo INI
        """
        try:
            config = configparser.ConfigParser()
            
            for section_name, section_data in data.items():
                config.add_section(section_name)
                for key, value in section_data.items():
                    config.set(section_name, key, str(value))
                    
            with open(file_path, 'w', encoding='utf-8') as file:
                config.write(file)
                
            return True
        except Exception as e:
            print(f"Error writing INI file {file_path}: {e}")
            return False
            
    def read_json_file(self, file_path: str) -> Dict[str, Any]:
        """
        Lee un archivo JSON y retorna su contenido
        """
        try:
            # Intentar diferentes codificaciones
            encodings = ['utf-8', 'latin-1', 'cp1252', 'iso-8859-1']
            
            for encoding in encodings:
                try:
                    with open(file_path, 'r', encoding=encoding) as file:
                        return json.load(file)
                except UnicodeDecodeError:
                    continue
            
            print(f"Error reading JSON file {file_path}: No se pudo decodificar con ninguna codificación")
            return {}
        except Exception as e:
            print(f"Error reading JSON file {file_path}: {e}")
            return {}
            
    def write_json_file(self, file_path: str, data: Dict[str, Any]) -> bool:
        """
        Escribe datos a un archivo JSON
        """
        try:
            with open(file_path, 'w', encoding='utf-8') as file:
                json.dump(data, file, indent=2, ensure_ascii=False)
            return True
        except Exception as e:
            print(f"Error writing JSON file {file_path}: {e}")
            return False
            
    def read_lua_file(self, file_path: str) -> str:
        """
        Lee un archivo LUA y retorna su contenido como string
        Nota: Para parsing completo de LUA se necesitaría una librería especializada
        """
        try:
            # Intentar diferentes codificaciones
            encodings = ['utf-8', 'latin-1', 'cp1252', 'iso-8859-1']
            
            for encoding in encodings:
                try:
                    with open(file_path, 'r', encoding=encoding) as file:
                        return file.read()
                except UnicodeDecodeError:
                    continue
            
            print(f"Error reading LUA file {file_path}: No se pudo decodificar con ninguna codificación")
            return ""
        except Exception as e:
            print(f"Error reading LUA file {file_path}: {e}")
            return ""
            
    def write_lua_file(self, file_path: str, content: str) -> bool:
        """
        Escribe contenido a un archivo LUA
        """
        try:
            with open(file_path, 'w', encoding='utf-8') as file:
                file.write(content)
            return True
        except Exception as e:
            print(f"Error writing LUA file {file_path}: {e}")
            return False
            
    def backup_config_file(self, file_path: str) -> bool:
        """
        Crea una copia de seguridad de un archivo de configuración
        """
        try:
            backup_path = f"{file_path}.backup"
            
            # Intentar diferentes codificaciones para leer el archivo original
            encodings = ['utf-8', 'latin-1', 'cp1252', 'iso-8859-1']
            content = None
            
            for encoding in encodings:
                try:
                    with open(file_path, 'r', encoding=encoding) as original:
                        content = original.read()
                        break
                except UnicodeDecodeError:
                    continue
            
            if content is None:
                print(f"Error creating backup: No se pudo decodificar el archivo {file_path} con ninguna codificación")
                return False
                
            with open(backup_path, 'w', encoding='utf-8') as backup:
                backup.write(content)
                
            return True
        except Exception as e:
            print(f"Error creating backup for {file_path}: {e}")
            return False
            
    def validate_ini_syntax(self, content: str) -> tuple[bool, str]:
        """
        Valida la sintaxis de un archivo INI
        Retorna (es_válido, mensaje_error)
        """
        try:
            # Verificar si el contenido tiene headers de sección
            has_sections = any(line.strip().startswith('[') and line.strip().endswith(']') 
                             for line in content.split('\n'))
            
            if has_sections:
                # Validar como INI normal con secciones
                config = configparser.ConfigParser(allow_no_value=True)
                config.read_string(content)
            else:
                # Validar como archivo de configuración sin secciones
                lines = content.strip().split('\n')
                for line in lines:
                    line = line.strip()
                    if line and not line.startswith('#') and '=' in line:
                        # Verificar que la línea tenga formato clave=valor válido
                        if line.count('=') < 1:
                            raise ValueError(f"Línea inválida: {line}")
            
            return True, "Sintaxis válida"
        except Exception as e:
            return False, f"Error de sintaxis: {e}"
            
    def validate_json_syntax(self, content: str) -> tuple[bool, str]:
        """
        Valida la sintaxis de un archivo JSON
        Retorna (es_válido, mensaje_error)
        """
        try:
            json.loads(content)
            return True, "Sintaxis válida"
        except json.JSONDecodeError as e:
            return False, f"Error de sintaxis JSON: {e}"
            
    def get_server_config_paths(self) -> Dict[str, str]:
        """
        Retorna las rutas de los archivos de configuración principales
        """
        base_path = self.server_path if self.server_path else "./server"
        
        return {
            "server_settings": os.path.join(base_path, "servertest.ini"),
            "sandbox_vars": os.path.join(base_path, "Lua", "shared", "sandbox_vars.lua"),
            "spawn_regions": os.path.join(base_path, "Lua", "shared", "spawnregions.lua"),
            "server_rules": os.path.join(base_path, "server_rules.json"),
            "admin_config": os.path.join(base_path, "admin.txt"),
        }
        
    def load_default_configs(self) -> Dict[str, str]:
        """
        Carga configuraciones por defecto para diferentes tipos de archivo
        """
        return {
            "server_settings": """# Project Zomboid Server Configuration
[Server]
PublicName=My PZ Server
PublicDescription=A Project Zomboid Server
MaxPlayers=32
PVP=true
PauseEmpty=true
PingLimit=400
HoursForLootRespawn=0
SaveWorldEveryMinutes=5
PlayerSafehouse=false
AdminSafehouse=false
SafehouseAllowTrepass=true
SafehouseAllowFire=true
SafehouseAllowLoot=true
SafehouseAllowRespawn=false
SafehouseDaySurvivedToClaim=0
SafeHouseRemovalTime=144
AllowDestructionBySledgehammer=true
KickFastPlayers=false
ServerPlayerID=76561198000000000
Port=16261
ResetID=123456789
""",
            "sandbox_vars": """-- Sandbox Variables Configuration
SandBoxVars = {
    Version = 5,
    -- Zombie Settings
    Zombies = 4, -- 1=Sprinters, 2=Fast Shamblers, 3=Shamblers, 4=Random
    Distribution = 1, -- 1=Urban Focused
    DayLength = 3, -- 1=15 min, 2=30 min, 3=1 hour, 4=2 hours, 5=Real-time
    StartMonth = 7, -- July
    StartTime = 2, -- 9 AM
    StartDay = 9, -- Day 9
    -- Survival Settings
    WaterShut = 6, -- 0-11 months, -1=never
    ElecShut = 6, -- 0-11 months, -1=never
    WaterShutModifier = 14, -- Days
    ElecShutModifier = 14, -- Days
    -- Loot Settings
    FoodLoot = 2, -- 1=Extremely Rare to 5=Abundant
    CannedFoodLoot = 2,
    LiteratureLoot = 2,
    SurvivalGearsLoot = 2,
    MedicalLoot = 2,
    WeaponLoot = 2,
    RangedWeaponLoot = 2,
    AmmoLoot = 2,
    MechanicsLoot = 2,
    OtherLoot = 2,
    -- Temperature
    Temperature = 3, -- 1=Very Cold to 5=Very Hot
    Rain = 3, -- 1=Very Dry to 5=Very Wet
    ErosionSpeed = 3, -- 1=Very Slow to 4=Very Fast
    -- Farming
    FarmingSpeed = 3, -- 1=Very Slow to 5=Very Fast
    PlantResilience = 3, -- 1=Very Low to 5=Very High
    PlantAbundance = 3, -- 1=Very Poor to 5=Very Abundant
    -- Compost
    CompostTime = 2, -- 1=1 Week to 6=6 Months
    -- Stats Decrease
    StatsDecrease = 3, -- 1=Very Slow to 5=Very Fast
    NatureAbundance = 3, -- 1=Very Poor to 5=Very Abundant
    Alarm = 4, -- 1=Never to 6=Very Often
    LockedHouses = 6, -- 1=Never to 6=Very Often
    -- Food Spoilage
    FoodSpoilage = 3, -- 1=Very Slow to 5=Very Fast
    FridgeFactor = 3, -- 1=Very Low to 5=Very High
    -- Loot Respawn
    LootRespawn = 1, -- 0=None, 1=Every Day, 2=Every Week, 3=Every Month
    -- Seen Hours for Loot Respawn
    SeenHoursPreventLootRespawn = 0,
    -- World Item Removal
    WorldItemRemovalList = "Base.Vest,Base.Shirt,Base.Blouse,Base.Sweater,Base.Hoodie",
    HoursForWorldItemRemoval = 24,
    ItemRemovalListBlacklistToggle = false,
    -- Corpse Removal
    TimeSinceApoCalendarStart = 0,
    -- Player vs Player
    PVP = true,
    -- Safety
    SafetySystem = true,
    ShowSafety = true,
    SafetyToggleTimer = 2,
    SafetyCooldownTimer = 10,
    -- Map
    MapAllKnown = false,
    MapRemotePlayer = true,
    -- Multiplayer
    MouseOverToSeeDisplayName = true,
    HidePlayersBehindYou = true,
    -- PVP Melee
    PVPMeleeWhileHitReaction = false,
    PVPMeleeDamageModifier = 30,
    -- PVP Firearm
    PVPFirearmDamageModifier = 50,
    -- Car Damage
    CarDamageOnImpact = 3,
    DamageToPlayerFromHitByACar = 1,
    -- Towing
    TrafficJam = true,
    CarAlarm = 2,
    PlayerDamageFromCrash = true,
    -- Audio
    SirenShutoffHours = 0,
    -- Raven Creek
    RavenCreekExpansion = false,
    -- Louisville
    LouisvilleExpansion = false,
    -- Multiplayer Faction
    Faction = true,
    FactionDaySurvivedToCreate = 0,
    FactionPlayersRequiredForTag = 1,
    -- Radio
    RadioTransmission = true,
    -- Helicopter
    HelicopterFreq = 2,
    -- MetaGame
    MetaEvent = 2,
    SleepingEvent = 1,
    -- Generator
    GeneratorSpawning = 3,
    GeneratorFuelConsumption = 1,
    -- Zombies Lore
    ZombieAttractionMultiplier = 1,
    -- Boredom/Unhappiness
    BoredomPreventionFactor = 2,
    UnhappinessPreventionFactor = 2,
    -- Stats
    StatsDecrease = 3,
    StarvationHealthLoss = 3,
    InjurySeverity = 2,
    BoneFracture = true,
    HoursForCorpseRemoval = 216,
    DecayingCorpseHealthImpact = 3,
    BloodLevel = 3,
    ClothingDegradation = 3,
    FireSpread = true,
    DaysForRottenFoodRemoval = -1,
    AllowExteriorGenerator = true,
    MaxFogIntensity = 1,
    MaxRainFxIntensity = 1,
    EnableSnowOnGround = true,
    MultiHitZombies = false,
    RearVulnerability = 3,
    AttackBlockMovements = true,
    AllClothesUnlocked = false,
    -- Character
    CharacterFreePoints = 0,
    ConstructionBonusPoints = 3,
    NightDarkness = 3,
    InjuryBloodLevel = 2,
    -- Nutrition
    Nutrition = true,
    -- Cooking
    Cooking = 2,
    -- Farming
    Farming = 3,
    -- Electrical
    ElectricalPower = true,
}
""",
            "spawn_regions": """-- Spawn Regions Configuration
function SpawnRegions()
    return {
        { name = "Muldraugh, KY", file = "media/maps/Muldraugh, KY/spawnpoints.lua" },
        { name = "West Point, KY", file = "media/maps/West Point, KY/spawnpoints.lua" },
        { name = "Riverside, KY", file = "media/maps/Riverside, KY/spawnpoints.lua" },
        { name = "Rosewood, KY", file = "media/maps/Rosewood, KY/spawnpoints.lua" },
    }
end
""",
            "server_rules": """{
  "server_info": {
    "name": "Project Zomboid Server",
    "description": "A survival server for Project Zomboid",
    "version": "1.0.0",
    "max_players": 32
  },
  "rules": [
    "No griefing or intentional destruction of other players' bases",
    "No cheating, hacking, or exploiting game bugs",
    "Respect other players and maintain a friendly environment",
    "No offensive language, racism, or harassment",
    "No spamming in chat or voice communications",
    "Follow admin instructions and decisions",
    "No real money trading of in-game items",
    "Report bugs and issues to administrators"
  ],
  "punishments": {
    "warning": {
      "description": "First offense - verbal warning",
      "duration": "immediate"
    },
    "kick": {
      "description": "Second offense - temporary removal from server",
      "duration": "immediate"
    },
    "temporary_ban": {
      "description": "Third offense - temporary ban",
      "duration": "24 hours to 7 days"
    },
    "permanent_ban": {
      "description": "Severe or repeated offenses",
      "duration": "permanent"
    }
  },
  "admin_contacts": [
    "Discord: YourDiscord#1234",
    "Steam: YourSteamProfile"
  ]
}
"""
        }