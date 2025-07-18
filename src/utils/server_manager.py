import subprocess
import psutil
import time
import os
from typing import Optional, Dict, Any
from datetime import datetime
from .config_loader import config_loader


class ServerManager:
    """
    Gestor para controlar el proceso del servidor Project Zomboid
    """
    
    def __init__(self, server_path: str = "", java_path: str = "java"):
        # Cargar configuración del servidor
        server_paths = config_loader.get_server_paths()
        if server_paths:
            self.server_path = server_paths.get('server_path', server_path)
            self.java_path = server_paths.get('java_path', java_path)
            self.config_files = server_paths.get('config_files', {})
        else:
            self.server_path = server_path
            self.java_path = java_path
            self.config_files = {}
        
        # Cargar configuración RCON
        self.rcon_config = config_loader.get_rcon_config() or {}
        
        # Cargar configuración del servidor actual
        self.current_server_config = config_loader.get_current_server_config() or {}
        
        self.server_process: Optional[subprocess.Popen] = None
        self.server_pid: Optional[int] = None
        self.start_time: Optional[datetime] = None
        
    def start_server(self, config_name: str = "servertest", memory_mb: int = 4096) -> bool:
        """
        Inicia el servidor Project Zomboid
        """
        if self.is_server_running():
            print("El servidor ya está ejecutándose")
            return False
            
        try:
            # Comando para iniciar el servidor
            cmd = [
                self.java_path,
                f"-Xmx{memory_mb}m",
                f"-Xms{memory_mb//2}m",
                "-Djava.awt.headless=true",
                "-XX:+UseG1GC",
                "-XX:+UnlockExperimentalVMOptions",
                "-XX:+UseG1GC",
                "-XX:G1NewSizePercent=20",
                "-XX:G1ReservePercent=20",
                "-XX:MaxGCPauseMillis=50",
                "-XX:G1HeapRegionSize=32m",
                "-cp", "java/istack-commons-runtime.jar;java/*",
                "zombie.network.GameServer",
                f"-servername {config_name}"
            ]
            
            # Cambiar al directorio del servidor si está especificado
            cwd = self.server_path if self.server_path else None
            
            # Iniciar el proceso
            self.server_process = subprocess.Popen(
                cmd,
                cwd=cwd,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                bufsize=1,
                universal_newlines=True
            )
            
            self.server_pid = self.server_process.pid
            self.start_time = datetime.now()
            
            # Esperar un momento para verificar que el proceso se inició correctamente
            time.sleep(2)
            
            if self.server_process.poll() is None:
                print(f"Servidor iniciado exitosamente (PID: {self.server_pid})")
                return True
            else:
                print("Error: El servidor se cerró inmediatamente después del inicio")
                return False
                
        except Exception as e:
            print(f"Error al iniciar el servidor: {e}")
            return False
            
    def stop_server(self, force: bool = False) -> bool:
        """
        Detiene el servidor Project Zomboid
        """
        if not self.is_server_running():
            print("El servidor no está ejecutándose")
            return True
            
        try:
            if force:
                # Forzar cierre del proceso
                if self.server_process:
                    self.server_process.terminate()
                    time.sleep(5)
                    if self.server_process.poll() is None:
                        self.server_process.kill()
                        
                # También intentar terminar por PID
                if self.server_pid:
                    try:
                        process = psutil.Process(self.server_pid)
                        process.terminate()
                        time.sleep(5)
                        if process.is_running():
                            process.kill()
                    except psutil.NoSuchProcess:
                        pass
            else:
                # Enviar comando de parada suave
                if self.server_process and self.server_process.stdin:
                    self.server_process.stdin.write("quit\n")
                    self.server_process.stdin.flush()
                    
                # Esperar hasta 30 segundos para cierre suave
                for _ in range(30):
                    if not self.is_server_running():
                        break
                    time.sleep(1)
                    
                # Si aún está ejecutándose, forzar cierre
                if self.is_server_running():
                    return self.stop_server(force=True)
                    
            self.server_process = None
            self.server_pid = None
            self.start_time = None
            
            print("Servidor detenido exitosamente")
            return True
            
        except Exception as e:
            print(f"Error al detener el servidor: {e}")
            return False
            
    def restart_server(self, config_name: str = "servertest", memory_mb: int = 4096) -> bool:
        """
        Reinicia el servidor Project Zomboid
        """
        print("Reiniciando servidor...")
        
        if self.is_server_running():
            if not self.stop_server():
                print("Error al detener el servidor para reinicio")
                return False
                
        # Esperar un momento antes de reiniciar
        time.sleep(3)
        
        return self.start_server(config_name, memory_mb)
        
    def is_server_running(self) -> bool:
        """
        Verifica si el servidor está ejecutándose
        """
        if self.server_process:
            return self.server_process.poll() is None
            
        if self.server_pid:
            try:
                process = psutil.Process(self.server_pid)
                return process.is_running()
            except psutil.NoSuchProcess:
                return False
                
        return False
        
    def get_server_stats(self) -> Dict[str, Any]:
        """
        Obtiene estadísticas del servidor
        """
        stats = {
            "running": False,
            "pid": None,
            "cpu_percent": 0.0,
            "memory_mb": 0.0,
            "uptime": "00:00:00",
            "players_connected": 0,
            "max_players": 32
        }
        
        if not self.is_server_running():
            return stats
            
        try:
            if self.server_pid:
                process = psutil.Process(self.server_pid)
                
                stats["running"] = True
                stats["pid"] = self.server_pid
                stats["cpu_percent"] = process.cpu_percent()
                stats["memory_mb"] = process.memory_info().rss / 1024 / 1024
                
                if self.start_time:
                    uptime_seconds = (datetime.now() - self.start_time).total_seconds()
                    hours = int(uptime_seconds // 3600)
                    minutes = int((uptime_seconds % 3600) // 60)
                    seconds = int(uptime_seconds % 60)
                    stats["uptime"] = f"{hours:02d}:{minutes:02d}:{seconds:02d}"
                    
        except psutil.NoSuchProcess:
            self.server_pid = None
            self.server_process = None
            
        return stats
        
    def send_command(self, command: str) -> bool:
        """
        Envía un comando al servidor
        """
        if not self.is_server_running() or not self.server_process:
            print("El servidor no está ejecutándose")
            return False
            
        try:
            if self.server_process.stdin:
                self.server_process.stdin.write(f"{command}\n")
                self.server_process.stdin.flush()
                return True
        except Exception as e:
            print(f"Error al enviar comando: {e}")
            
        return False
        
    def get_server_log(self, lines: int = 100) -> str:
        """
        Obtiene las últimas líneas del log del servidor
        """
        if not self.server_process:
            return "Servidor no está ejecutándose"
            
        try:
            # En una implementación real, esto leería del archivo de log
            # Por ahora retornamos un log de ejemplo
            return "Log del servidor no disponible en esta implementación de ejemplo"
        except Exception as e:
            return f"Error al leer log: {e}"
            
    def backup_server_data(self, backup_path: str) -> bool:
        """
        Crea un respaldo de los datos del servidor
        """
        try:
            # TODO: Implementar lógica de respaldo real
            # Esto incluiría copiar archivos de mundo, configuraciones, etc.
            print(f"Creando respaldo en: {backup_path}")
            return True
        except Exception as e:
            print(f"Error al crear respaldo: {e}")
            return False
            
    def get_player_list(self) -> list:
        """
        Obtiene la lista de jugadores conectados
        """
        # TODO: Implementar obtención real de lista de jugadores
        # Esto requeriría parsear logs o usar RCON
        return []
        
    def kick_player(self, username: str) -> bool:
        """
        Expulsa a un jugador del servidor
        """
        return self.send_command(f"kickuser {username}")
        
    def ban_player(self, username: str) -> bool:
        """
        Banea a un jugador del servidor
        """
        return self.send_command(f"banuser {username}")
        
    def unban_player(self, username: str) -> bool:
        """
        Desbanea a un jugador
        """
        return self.send_command(f"unbanuser {username}")
        
    def broadcast_message(self, message: str) -> bool:
        """
        Envía un mensaje a todos los jugadores
        """
        return self.send_command(f"servermsg {message}")