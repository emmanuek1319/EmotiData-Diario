# 🧠 EmotiData - Diario Inteligente

Aplicación de Diario Inteligente con análisis de emociones, sistema de rachas y panel clínico de acompañamiento psicológico.

## 🛠️ Tecnologías utilizadas
* **Frontend:** C# (.NET Windows Forms)
* **Backend / API:** Python (Flask)
* **Base de Datos:** MongoDB (Local)

## 🚀 Características principales
* **Análisis de Emociones:** Detección inteligente de emociones (Alegría, Tristeza, Enojo, Ansiedad) mediante análisis de texto.
* **Gamificación:** Sistema de rachas, niveles y medallas para fomentar la constancia del estudiante.
* **Módulo Especialista:** Interfaz clínica exclusiva para psicólogos donde pueden dar de alta estudiantes, revisar historiales, gráficas de puntuación y generar reportes dinámicos.
* **Personalización:** 7 temas visuales dinámicos.
* **Zona Relax:** Espacio de contención con frases motivacionales de filosofía estoica.

## ⚙️ Configuración y Ejecución
1. Asegúrate de tener MongoDB ejecutándose de forma local en el puerto `27017`.
2. Instala las dependencias de Python: `pip install flask flask-cors pymongo`
3. Ejecuta la API: `python diario.py`
4. Compila y ejecuta la aplicación de escritorio en C# desde Visual Studio.
