// EmotiData · Puente de correo con Google Apps Script (se pega en script.google.com con la cuenta de EmotiData).
// 1) Configuración del proyecto → Propiedades del script → agrega CLAVE = una cadena larga y aleatoria (mínimo 16 caracteres).
// 2) Implementar → Nueva implementación → Aplicación web · Ejecutar como: Yo · Quién tiene acceso: Cualquier persona.
// 3) Copia la URL que termina en /exec en MAIL_WEBHOOK_URL (Render) y la misma CLAVE en MAIL_WEBHOOK_KEY.
// Sin la CLAVE nadie puede usar tu Gmail para enviar mensajes: cualquier petición sin ella se rechaza.
function doPost(e) {
  try {
    const d = JSON.parse(e.postData.contents);
    const clave = PropertiesService.getScriptProperties().getProperty("CLAVE");
    if (!clave || d.clave !== clave) return salida({ ok: false, error: "no autorizado" });
    if (!/^[^@\s]{1,64}@[^@\s]{1,120}\.[^@\s]{2,}$/.test(String(d.para))) return salida({ ok: false, error: "correo invalido" });
    MailApp.sendEmail({
      to: String(d.para),
      subject: String(d.asunto).slice(0, 120),
      body: String(d.texto).slice(0, 4000),
      htmlBody: String(d.html).slice(0, 20000),
      name: "EmotiData"
    });
    return salida({ ok: true });
  } catch (err) {
    return salida({ ok: false, error: "error" });
  }
}
function salida(o) {
  return ContentService.createTextOutput(JSON.stringify(o)).setMimeType(ContentService.MimeType.JSON);
}
