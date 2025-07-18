import os
import platform
from pathlib import Path
from typing import Dict, List, Optional

class PlatformUtils:
    """
    Utilidades para manejo multiplataforma de rutas y configuraciones
    """
    
    def __init__(self):
        self.system = platform.system().lower()
        self.is_windows = self.system == 'windows'
        self.is_linux = self.system == 'linux'
        self.is_macos = self.system == 'darwin'
    
    def get_system_info(self) -> Dict[str, str]:
        """
        Obtiene información del sistema operativo
        """
        return {
            'system': self.system,
            'platform': platform.platform(),
            'architecture': platform.architecture()[0],
            'python_version': platform.python_version()
        }
    
    def normalize_path(self, path: str) -> str:
        """
        Normaliza una ruta para el sistema operativo actual
        
        Args:
            path: Ruta a normalizar
            
        Returns:
            Ruta normalizada para el sistema actual
        """
        if not path:
            return path
        
        # Expandir variables de entorno
        expanded_path = os.path.expandvars(path)
        expanded_path = os.path.expanduser(expanded_path)
        
        # Convertir separadores de ruta
        normalized = Path(expanded_path).as_posix() if not self.is_windows else str(Path(expanded_path))
        
        return normalized
    
    def get_default_zomboid_paths(self) -> Dict[str, str]:
        """
        Obtiene las rutas por defecto de Project Zomboid según el sistema operativo
        
        Returns:
            Diccionario con las rutas por defecto
        """
        if self.is_windows:
            return {
                'server_path': os.path.expandvars("C:\\Users\\%USERNAME%\\Zomboid\\Server"),
                'game_path': "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ProjectZomboid",
                'executable': "ProjectZomboid64.exe",
                'save_path': os.path.expandvars("C:\\Users\\%USERNAME%\\Zomboid\\Saves")
            }
        elif self.is_linux:
            return {
                'server_path': os.path.expanduser("~/.steam/steam/steamapps/common/ProjectZomboid/server"),
                'game_path': os.path.expanduser("~/.steam/steam/steamapps/common/ProjectZomboid"),
                'executable': "ProjectZomboid64",
                'save_path': os.path.expanduser("~/Zomboid/Saves")
            }
        elif self.is_macos:
            return {
                'server_path': os.path.expanduser("~/Library/Application Support/Steam/steamapps/common/ProjectZomboid/server"),
                'game_path': os.path.expanduser("~/Library/Application Support/Steam/steamapps/common/ProjectZomboid"),
                'executable': "ProjectZomboid64",
                'save_path': os.path.expanduser("~/Zomboid/Saves")
            }
        else:
            # Fallback genérico
            return {
                'server_path': os.path.expanduser("~/Zomboid/Server"),
                'game_path': os.path.expanduser("~/ProjectZomboid"),
                'executable': "ProjectZomboid64",
                'save_path': os.path.expanduser("~/Zomboid/Saves")
            }
    
    def find_steam_installations(self) -> List[str]:
        """
        Busca instalaciones de Steam en el sistema
        
        Returns:
            Lista de rutas donde se encontró Steam
        """
        steam_paths = []
        
        if self.is_windows:
            possible_paths = [
                "C:\\Program Files (x86)\\Steam",
                "C:\\Program Files\\Steam",
                "D:\\Steam",
                "E:\\Steam",
                "F:\\Steam"
            ]
        elif self.is_linux:
            possible_paths = [
                os.path.expanduser("~/.steam/steam"),
                os.path.expanduser("~/.local/share/Steam"),
                "/usr/games/steam",
                "/opt/steam"
            ]
        elif self.is_macos:
            possible_paths = [
                os.path.expanduser("~/Library/Application Support/Steam")
            ]
        else:
            possible_paths = []
        
        for path in possible_paths:
            if os.path.exists(path):
                steam_paths.append(path)
        
        return steam_paths
    
    def find_pz_executable(self, custom_paths: List[str] = None) -> Optional[str]:
        """
        Busca el ejecutable de Project Zomboid
        
        Args:
            custom_paths: Rutas personalizadas donde buscar
            
        Returns:
            Ruta al ejecutable si se encuentra, None en caso contrario
        """
        search_paths = custom_paths or []
        
        # Agregar rutas por defecto
        default_paths = self.get_default_zomboid_paths()
        search_paths.append(default_paths['game_path'])
        
        # Agregar rutas de Steam
        for steam_path in self.find_steam_installations():
            pz_path = os.path.join(steam_path, "steamapps", "common", "ProjectZomboid")
            search_paths.append(pz_path)
        
        executable_name = default_paths['executable']
        
        for path in search_paths:
            if not path or not os.path.exists(path):
                continue
            
            executable_path = os.path.join(path, executable_name)
            if os.path.exists(executable_path):
                return self.normalize_path(executable_path)
        
        return None
    
    def validate_server_files(self, server_path: str, server_name: str) -> Dict[str, bool]:
        """
        Valida que existan los archivos de configuración de un servidor
        El archivo .ini es obligatorio, los archivos .lua son opcionales
        
        Args:
            server_path: Ruta del directorio del servidor
            server_name: Nombre del servidor
            
        Returns:
            Diccionario indicando qué archivos existen
        """
        server_files = {
            'server_settings': f"{server_name}.ini",
            'sandbox_vars': f"{server_name}_SandBoxVars.lua",
            'spawn_points': f"{server_name}_spawnpoints.lua",
            'spawn_regions': f"{server_name}_spawnregions.lua"
        }
        
        validation_result = {}
        
        for file_type, filename in server_files.items():
            file_path = os.path.join(server_path, filename)
            validation_result[file_type] = os.path.exists(file_path)
        
        return validation_result
    
    def is_valid_server(self, server_path: str, server_name: str) -> bool:
        """
        Verifica si un servidor es válido
        Solo requiere el archivo .ini como obligatorio
        
        Args:
            server_path: Ruta del directorio del servidor
            server_name: Nombre del servidor
            
        Returns:
            True si el servidor es válido (tiene al menos el archivo .ini)
        """
        validation = self.validate_server_files(server_path, server_name)
        # Solo requiere el archivo .ini como obligatorio
        return validation.get('server_settings', False)
    
    def get_path_separator(self) -> str:
        """
        Obtiene el separador de rutas del sistema actual
        """
        return os.sep
    
    def join_paths(self, *paths) -> str:
        """
        Une rutas de manera compatible con el sistema operativo
        """
        return self.normalize_path(os.path.join(*paths))
    
    def create_directory(self, path: str) -> bool:
        """
        Crea un directorio de manera segura
        
        Args:
            path: Ruta del directorio a crear
            
        Returns:
            True si se creó exitosamente o ya existía
        """
        try:
            normalized_path = self.normalize_path(path)
            os.makedirs(normalized_path, exist_ok=True)
            return True
        except Exception as e:
            print(f"Error al crear directorio {path}: {e}")
            return False

# Instancia global de utilidades de plataforma
platform_utils = PlatformUtils()