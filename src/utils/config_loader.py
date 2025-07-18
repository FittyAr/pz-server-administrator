import json
import os
from pathlib import Path
from typing import Dict, Any, Optional

class ConfigLoader:
    """
    Clase para cargar y gestionar la configuración de la aplicación desde config.json
    """
    
    def __init__(self, config_path: str = None):
        """
        Inicializa el cargador de configuración
        
        Args:
            config_path: Ruta al archivo de configuración. Si es None, usa la ruta por defecto.
        """
        if config_path is None:
            # Busca el archivo config.json en la raíz del proyecto
            current_dir = Path(__file__).parent.parent.parent
            self.config_path = current_dir / "config.json"
        else:
            self.config_path = Path(config_path)
            
        self.config_data: Dict[str, Any] = {}
        self._load_config()
    
    def _load_config(self) -> None:
        """
        Carga la configuración desde el archivo JSON
        """
        try:
            if self.config_path.exists():
                with open(self.config_path, 'r', encoding='utf-8') as file:
                    self.config_data = json.load(file)
                self._expand_environment_variables()
                print(f"Configuración cargada desde: {self.config_path}")
            else:
                print(f"Archivo de configuración no encontrado: {self.config_path}")
                self._create_default_config()
        except json.JSONDecodeError as e:
            print(f"Error al parsear el archivo de configuración: {e}")
            self._create_default_config()
        except Exception as e:
            print(f"Error al cargar la configuración: {e}")
            self._create_default_config()
    
    def _expand_environment_variables(self) -> None:
        """
        Expande las variables de entorno en las rutas de configuración
        """
        def expand_vars(obj):
            if isinstance(obj, str):
                return os.path.expandvars(obj)
            elif isinstance(obj, dict):
                return {key: expand_vars(value) for key, value in obj.items()}
            elif isinstance(obj, list):
                return [expand_vars(item) for item in obj]
            return obj
        
        self.config_data = expand_vars(self.config_data)
    
    def _create_default_config(self) -> None:
        """
        Crea una configuración por defecto si no existe el archivo
        """
        self.config_data = {
            "app_config": {
                "app_name": "PZ Server Administrator",
                "version": "1.0.0",
                "language": "es",
                "theme": "dark",
                "auto_save_config": True,
                "backup_retention_days": 30
            },
            "server_config": {
                "default_server": "main_server",
                "servers": {
                    "main_server": {
                        "name": "Servidor Principal",
                        "description": "Servidor principal de Project Zomboid",
                        "server_path": "C:\\Users\\%USERNAME%\\Zomboid\\Server",
                        "executable_path": "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ProjectZomboid\\ProjectZomboid64.exe",
                        "rcon": {
                            "enabled": True,
                            "host": "127.0.0.1",
                            "port": 27015,
                            "password": ""
                        }
                    }
                }
            }
        }
    
    def get_config(self, section: str = None, key: str = None) -> Any:
        """
        Obtiene un valor de configuración
        
        Args:
            section: Sección de configuración (ej: 'app_config', 'server_config')
            key: Clave específica dentro de la sección
            
        Returns:
            El valor de configuración solicitado o None si no existe
        """
        if section is None:
            return self.config_data
        
        if section not in self.config_data:
            return None
            
        if key is None:
            return self.config_data[section]
            
        return self.config_data[section].get(key)
    
    def get_app_config(self, key: str = None) -> Any:
        """
        Obtiene configuración de la aplicación
        """
        return self.get_config('app_config', key)
    
    def get_server_config(self, key: str = None) -> Any:
        """
        Obtiene configuración del servidor
        """
        return self.get_config('server_config', key)
    
    def get_current_server_config(self) -> Optional[Dict[str, Any]]:
        """
        Obtiene la configuración del servidor actual (predeterminado)
        """
        server_config = self.get_server_config()
        if not server_config:
            return None
            
        default_server = server_config.get('default_server')
        if not default_server:
            return None
            
        servers = server_config.get('servers', {})
        return servers.get(default_server)
    
    def get_backup_config(self, key: str = None) -> Any:
        """
        Obtiene configuración de respaldos
        """
        return self.get_config('backup_config', key)
    
    def get_logging_config(self, key: str = None) -> Any:
        """
        Obtiene configuración de logging
        """
        return self.get_config('logging_config', key)
    
    def get_ui_preferences(self, key: str = None) -> Any:
        """
        Obtiene preferencias de UI
        """
        return self.get_config('ui_preferences', key)
    
    def get_rcon_config(self) -> Optional[Dict[str, Any]]:
        """
        Obtiene la configuración RCON del servidor actual
        """
        current_server = self.get_current_server_config()
        if current_server:
            return current_server.get('rcon')
        return None
    
    def get_server_paths(self) -> Optional[Dict[str, str]]:
        """
        Obtiene las rutas importantes del servidor actual
        """
        current_server = self.get_current_server_config()
        if current_server:
            return {
                'server_path': current_server.get('server_path', ''),
                'executable_path': current_server.get('executable_path', ''),
                'config_files': current_server.get('config_files', {})
            }
        return None
    
    def validate_config(self) -> Dict[str, list]:
        """
        Valida la configuración y retorna una lista de errores/advertencias
        
        Returns:
            Diccionario con 'errors' y 'warnings'
        """
        errors = []
        warnings = []
        
        # Validar configuración del servidor
        current_server = self.get_current_server_config()
        if current_server:
            server_path = current_server.get('server_path')
            if server_path and not Path(server_path).exists():
                warnings.append(f"La ruta del servidor no existe: {server_path}")
            
            executable_path = current_server.get('executable_path')
            if executable_path and not Path(executable_path).exists():
                warnings.append(f"El ejecutable del servidor no existe: {executable_path}")
            
            # Validar configuración RCON
            rcon_config = current_server.get('rcon', {})
            if rcon_config.get('enabled'):
                if not rcon_config.get('password'):
                    warnings.append("RCON está habilitado pero no se ha configurado contraseña")
                
                port = rcon_config.get('port')
                if not isinstance(port, int) or port < 1 or port > 65535:
                    errors.append(f"Puerto RCON inválido: {port}")
        else:
            errors.append("No se encontró configuración del servidor predeterminado")
        
        # Validar configuración de respaldos
        backup_config = self.get_backup_config()
        if backup_config:
            backup_path = backup_config.get('backup_path')
            if backup_path:
                backup_dir = Path(backup_path)
                if not backup_dir.exists():
                    try:
                        backup_dir.mkdir(parents=True, exist_ok=True)
                    except Exception:
                        warnings.append(f"No se puede crear el directorio de respaldos: {backup_path}")
        
        return {'errors': errors, 'warnings': warnings}
    
    def save_config(self) -> bool:
        """
        Guarda la configuración actual al archivo JSON
        
        Returns:
            True si se guardó exitosamente, False en caso contrario
        """
        try:
            with open(self.config_path, 'w', encoding='utf-8') as file:
                json.dump(self.config_data, file, indent=2, ensure_ascii=False)
            print(f"Configuración guardada en: {self.config_path}")
            return True
        except Exception as e:
            print(f"Error al guardar la configuración: {e}")
            return False
    
    def update_config(self, section: str, key: str, value: Any) -> bool:
        """
        Actualiza un valor de configuración
        
        Args:
            section: Sección de configuración
            key: Clave a actualizar
            value: Nuevo valor
            
        Returns:
            True si se actualizó exitosamente
        """
        try:
            if section not in self.config_data:
                self.config_data[section] = {}
            
            self.config_data[section][key] = value
            
            # Auto-guardar si está habilitado
            if self.get_app_config('auto_save_config'):
                return self.save_config()
            
            return True
        except Exception as e:
            print(f"Error al actualizar configuración: {e}")
            return False

# Instancia global del cargador de configuración
config_loader = ConfigLoader()