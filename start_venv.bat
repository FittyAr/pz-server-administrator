@echo off
REM Script para iniciar el entorno virtual de Python
REM Autor: Administrador del Servidor PZ

echo Iniciando entorno virtual de Python...

REM Verificar si existe el directorio del entorno virtual
if not exist "venv" (
    echo El entorno virtual no existe. Creando entorno virtual...
    python -m venv venv
    if errorlevel 1 (
        echo Error: No se pudo crear el entorno virtual.
        echo Asegurate de tener Python instalado y agregado al PATH.
        pause
        exit /b 1
    )
    echo Entorno virtual creado exitosamente.
)

REM Activar el entorno virtual
echo Activando entorno virtual...
call venv\Scripts\activate.bat

if errorlevel 1 (
    echo Error: No se pudo activar el entorno virtual.
    pause
    exit /b 1
)

echo.
echo ========================================
echo  Entorno virtual activado exitosamente
echo ========================================
echo.
echo Para desactivar el entorno virtual, escribe: deactivate
echo Para instalar dependencias, usa: pip install -r requirements.txt
echo.

REM Mantener la ventana abierta
cmd /k