document.addEventListener("DOMContentLoaded", function () {
  showTab("disease");
  getDiseaseList();
  getSymptomList();

  const menuToggle = document.querySelector('.avatar-top');
  menuToggle.addEventListener("click", togglePanel);
  const overlay = document.querySelector('.overlay');
  overlay.addEventListener("click", togglePanel);
});

function showTab(tabName) {
  const diseasePanel = document.getElementById("diseasePanel");
  const symptomPanel = document.getElementById("symptomPanel");
  const tabDisease = document.getElementById("tabDisease");
  const tabSymptom = document.getElementById("tabSymptom");
  if (tabName === "disease") {
    diseasePanel.style.display = "block";
    symptomPanel.style.display = "none";
    tabDisease.classList.add("active");
    tabSymptom.classList.remove("active");
  } else {
    diseasePanel.style.display = "none";
    symptomPanel.style.display = "block";
    tabSymptom.classList.add("active");
    tabDisease.classList.remove("active");
  }
}

function getDiseaseList() {
  window.chrome.webview.postMessage({ action: "getDiseaseList" });
}
function getSymptomList() {
  window.chrome.webview.postMessage({ action: "getSymptomList" });
}

function renderDiseaseList(diseases) {
  const container = document.getElementById("diseaseList");
  container.innerHTML = "";
  diseases.forEach(d => {
    const div = document.createElement("div");
    div.className = "record";
    div.innerHTML = `<div>
      <strong>${d.Name}</strong><br>
      ${d.Description}
    </div>
    <div class="record-actions">
      <button class="edit-btn" onclick="editDisease('${d.Id}','${d.Name}','${d.Description}','${d.SymptomIds}')">Редактировать</button>
      <button class="delete-btn" onclick="deleteDisease('${d.Id}')">Удалить</button>
    </div>`;
    container.appendChild(div);
  });
}

function renderSymptomList(symptoms) {
  const container = document.getElementById("symptomList");
  container.innerHTML = "";
  symptoms.forEach(s => {
    const div = document.createElement("div");
    div.className = "record";
    div.innerHTML = `<div>
      <strong>${s.Name}</strong>
    </div>
    <div class="record-actions">
      <button class="edit-btn" onclick="editSymptom('${s.Id}','${s.Name}','${s.DiseaseId}')">Редактировать</button>
      <button class="delete-btn" onclick="deleteSymptom('${s.Id}')">Удалить</button>
    </div>`;
    container.appendChild(div);
  });
}

function saveDisease() {
  const id = document.getElementById("diseaseId").value;
  const name = document.getElementById("diseaseName").value;
  const description = document.getElementById("diseaseDescription").value;
  const symptomIdsStr = document.getElementById("diseaseSymptomIds").value;
  const symptomIds = symptomIdsStr.split(",").map(s => s.trim()).filter(s => s !== "");
  const actionType = id ? "updateDisease" : "addDisease";
  window.chrome.webview.postMessage({
    action: actionType,
    id: id,
    name: name,
    description: description,
    symptomIds: symptomIds
  });
}

function resetDiseaseForm() {
  document.getElementById("diseaseForm").reset();
  document.getElementById("diseaseId").value = "";
}

function editDisease(id, name, description, symptomIds) {
  document.getElementById("diseaseId").value = id;
  document.getElementById("diseaseName").value = name;
  document.getElementById("diseaseDescription").value = description;
  document.getElementById("diseaseSymptomIds").value = symptomIds;
}

function deleteDisease(id) {
  if (confirm("Вы действительно хотите удалить болезнь?")) {
    window.chrome.webview.postMessage({
      action: "deleteDisease",
      id: id
    });
  }
}


function saveSymptom() {
  const id = document.getElementById("symptomId").value;
  const name = document.getElementById("symptomName").value;
  const diseaseId = document.getElementById("symptomDiseaseId").value;
  if (!diseaseId || diseaseId.trim() === "") {
    alert(Resources.AdminPanelResources_EnterDiseaseIdForSymptom);
    return;
  }
  const actionType = id ? "updateSymptom" : "addSymptom";
  window.chrome.webview.postMessage({
    action: actionType,
    id: id,
    name: name,
    diseaseId: diseaseId
  });
}

function resetSymptomForm() {
  document.getElementById("symptomForm").reset();
  document.getElementById("symptomId").value = "";
}

function editSymptom(id, name, diseaseId) {
  document.getElementById("symptomId").value = id;
  document.getElementById("symptomName").value = name;
  document.getElementById("symptomDiseaseId").value = diseaseId;
}

function deleteSymptom(id) {
  if (confirm("Вы действительно хотите удалить симптом?")) {
    window.chrome.webview.postMessage({
      action: "deleteSymptom",
      id: id
    });
  }
}

window.chrome.webview.addEventListener("message", event => {
  const message = event.data;
  if (message.type === "diseaseList") {
    renderDiseaseList(message.data);
  }
  if (message.type === "symptomList") {
    renderSymptomList(message.data);
  }
  if (message.type === "success") {
    alert(message.message);
    getDiseaseList();
    getSymptomList();
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
function logout() {
    window.chrome.webview.postMessage({ action: "logout" });
}
