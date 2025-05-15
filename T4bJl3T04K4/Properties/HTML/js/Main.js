function onSystemChange() {
      const system = document.getElementById("systemSelect").value;
      if (system) {
        window.chrome.webview.postMessage({ action: "getSymptoms", system });
      }

      // Сброс старых результатов
      document.getElementById("symptomSection").style.display = "none";
      document.getElementById("symptomCheckboxes").innerHTML = "";
      const result = document.getElementById("diagnosisResult");
      result.style.display = "none";
      result.innerHTML = "";
    }

    function renderSymptoms(symptomList) {
      const section = document.getElementById("symptomSection");
      const container = document.getElementById("symptomCheckboxes");
      section.style.display = "block";
      container.innerHTML = "";

      symptomList.forEach(symptom => {
        const label = document.createElement("label");
        label.innerHTML = `<input type="checkbox" data-id="${symptom.id}"> ${symptom.name}`;
        container.appendChild(label);
      });
    }

    function diagnose() {
      const checked = document.querySelectorAll("#symptomCheckboxes input:checked");
      const selectedIds = Array.from(checked).map(cb => cb.dataset.id);

      if (selectedIds.length === 0) {
        alert("Пожалуйста, выберите хотя бы один симптом.");
        return;
      }

      window.chrome.webview.postMessage({
        action: "diagnose",
        selectedSymptomIds: selectedIds
      });
    }

    function renderDiagnosis(results) {
      const result = document.getElementById("diagnosisResult");
      if (results.length === 0) {
        result.innerHTML = "Нет совпадений. Обратитесь к врачу.";
      } else {
        result.innerHTML = "<strong>Возможные диагнозы:</strong><ul>" +
          results.map(r => `<li>${r.Name} — совпадений: ${r.MatchCount}/${r.Total}</li>`).join('') +
          "</ul>";
      }
      result.style.display = "block";
    }

    window.chrome.webview.addEventListener('message', event => {
      const message = event.data;

      if (message.type === "symptoms") {
        renderSymptoms(message.data);
      }

      if (message.type === "diagnosis") {
        renderDiagnosis(message.results);
      }

      if (message.type === "error") {
        alert("Ошибка: " + message.message);
      }
    });
    function togglePanel() {
      const overlay = document.querySelector('.overlay');
      const panel = document.getElementById('sidePanel');
      if (overlay.style.display === 'block') {
        overlay.style.display = 'none';
        panel.style.right = '-300px';
      } else {
        overlay.style.display = 'block';
        panel.style.right = '0';
      }
    }

    function toggleSubmenu(id) {
      const submenu = document.getElementById(id);
      submenu.style.display = submenu.style.display === 'block' ? 'none' : 'block';
    }