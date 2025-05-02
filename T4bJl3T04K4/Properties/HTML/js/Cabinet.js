document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('profileForm');
    form.addEventListener('submit', handleSubmit);
  
    document.getElementById('uploadBtn').addEventListener('click', uploadPhoto);
    document.getElementById('deleteBtn').addEventListener('click', deletePhoto);
    document.getElementById('cancelBtn').addEventListener('click', resetForm);
  });
  
  function handleSubmit(e) {
    e.preventDefault();
    
    const formData = {
      username: document.getElementById('username').value,
      lastname: document.getElementById('lastname').value,
      firstname: document.getElementById('firstname').value,
      gender: document.getElementById('gender').value,
      birthdate: document.getElementById('birthdate').value
    };
  
    console.log('Данные формы:', formData);
    alert('Данные успешно сохранены!');
    resetForm();
  }
  
  function resetForm() {
    document.getElementById('profileForm').reset();
  }
  
  function uploadPhoto() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    
    input.onchange = e => {
      const file = e.target.files[0];
      if (file) {
        const reader = new FileReader();
        reader.onload = event => {
          document.querySelector('.profile-pic img').src = event.target.result;
        };
        reader.readAsDataURL(file);
      }
    };
    
    input.click();
  }
  
  function deletePhoto() {
    if (confirm('Вы уверены, что хотите удалить фото?')) {
      document.querySelector('.profile-pic img').src = 'img/default-avatar.jpg';
    }
  }