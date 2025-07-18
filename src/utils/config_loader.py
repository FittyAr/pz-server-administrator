import json
import os
from pathlib import Path
from typing import Dict, Any, Optional
from .platform_utils import platform_utils

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
    
    def scan_server_directory(self, base_path: str = None) -> Dict[str, Dict[str, Any]]:
        """
        Escanea un directorio en busca de configuraciones de servidores de Project Zomboid
        
        Args:
            base_path: Ruta base donde buscar servidores. Si es None, usa la configuración o ruta por defecto.
            
        Returns:
            Diccionario con los servidores encontrados
        """
        if base_path is None:
            # Intentar obtener la ruta configurada por el usuario
            server_config = self.get_server_config()
            if server_config and 'custom_server_path' in server_config:
                base_path = server_config['custom_server_path']
            else:
                # Usar ruta por defecto del sistema
                default_paths = platform_utils.get_default_zomboid_paths()
                base_path = default_paths['server_path']
        
        # Normalizar la ruta para el sistema actual
        base_path = platform_utils.normalize_path(base_path)
        servers_found = {}
        
        try:
            if not os.path.exists(base_path):
                print(f"Directorio de servidores no encontrado: {base_path}")
                return servers_found
            
            print(f"Escaneando directorio: {base_path}")
            
            # Buscar archivos .ini que indican configuraciones de servidor
            for item in os.listdir(base_path):
                if item.endswith('.ini'):
                    server_name = item.replace('.ini', '')
                    server_id = server_name.lower().replace(' ', '_').replace('-', '_')
                    
                    # Verificar que el servidor tenga los 4 archivos obligatorios
                    if platform_utils.is_valid_server(base_path, server_name):
                        # Obtener información de validación para mostrar qué archivos existen
                        validation = platform_utils.validate_server_files(base_path, server_name)
                        
                        config_files = {
                            'server_settings': f"{server_name}.ini",
                            'sandbox_vars': f"{server_name}_SandBoxVars.lua",
                            'spawn_regions': f"{server_name}_spawnregions.lua",
                            'spawn_points': f"{server_name}_spawnpoints.lua"
                        }
                        
                        servers_found[server_id] = {
                            'name': server_name,
                            'description': f"Servidor {server_name} detectado automáticamente",
                            'server_path': base_path,
                            'executable_path': self._find_pz_executable(),
                            'config_files': config_files,
                            'auto_detected': True,
                            'valid': True,
                            'validation': validation,
                            'rcon': {
                                'enabled': False,
                                'host': '127.0.0.1',
                                'port': 27015,
                                'password': ''
                            },
                            'game_server': {
                                'host': '0.0.0.0',
                                'port': 16261,
                                'udp_port': 16262
                            },
                            'steam_integration': {
                                'enabled': False,
                                'steam_port1': 8766,
                                'steam_port2': 8767
                            }
                        }
                    else:
                        # Servidor incompleto - agregar con información de validación
                        validation = platform_utils.validate_server_files(base_path, server_name)
                        missing_files = [file_type for file_type, exists in validation.items() if not exists]
                        
                        print(f"Servidor incompleto '{server_name}': faltan archivos {missing_files}")
                        
                        servers_found[f"{server_id}_incomplete"] = {
                            'name': f"{server_name} (Incompleto)",
                            'description': f"Servidor {server_name} - Faltan archivos: {', '.join(missing_files)}",
                            'server_path': base_path,
                            'executable_path': self._find_pz_executable(),
                            'config_files': {},
                            'auto_detected': True,
                            'valid': False,
                            'validation': validation,
                            'missing_files': missing_files
                        }
            
            valid_servers = [k for k, v in servers_found.items() if v.get('valid', False)]
            invalid_servers = [k for k, v in servers_found.items() if not v.get('valid', False)]
            
            print(f"Servidores válidos detectados: {valid_servers}")
            if invalid_servers:
                print(f"Servidores incompletos detectados: {invalid_servers}")
            
        except Exception as e:
            print(f"Error al escanear directorio de servidores: {e}")
        
        return servers_found
    
    def _find_pz_executable(self) -> str:
        """
        Intenta encontrar el ejecutable de Project Zomboid usando las utilidades de plataforma
        """
        # Intentar obtener ruta personalizada de la configuración
        server_config = self.get_server_config()
        custom_paths = []
        
        if server_config and 'custom_game_path' in server_config:
            custom_paths.append(server_config['custom_game_path'])
        
        # Buscar usando las utilidades de plataforma
        executable_path = platform_utils.find_pz_executable(custom_paths)
        
        if executable_path:
            return executable_path
        
        # Fallback con nombre apropiado para el sistema
        default_paths = platform_utils.get_default_zomboid_paths()
        return default_paths['executable']
    
    def get_all_servers(self) -> Dict[str, Dict[str, Any]]:
        """
        Obtiene todos los servidores configurados y detectados
        """
        # Servidores configurados manualmente
        configured_servers = self.get_server_config('servers') or {}
        
        # Servidores detectados automáticamente
        detected_servers = self.scan_server_directory()
        
        # Combinar ambos, dando prioridad a los configurados manualmente
        all_servers = detected_servers.copy()
        all_servers.update(configured_servers)
        
        return all_servers
    
    def set_default_server(self, server_id: str) -> bool:
        """
        Establece un servidor como predeterminado (favorito)
        
        Args:
            server_id: ID del servidor a establecer como predeterminado
            
        Returns:
            True si se estableció exitosamente
        """
        all_servers = self.get_all_servers()
        
        if server_id not in all_servers:
            print(f"Servidor no encontrado: {server_id}")
            return False
        
        return self.update_config('server_config', 'default_server', server_id)
    
    def get_default_server_id(self) -> Optional[str]:
        """
        Obtiene el ID del servidor predeterminado
        """
        return self.get_server_config('default_server')
    
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