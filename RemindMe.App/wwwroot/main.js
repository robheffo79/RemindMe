async function loadTasks() {
  const res = await fetch('/api/tasks');
  const tasks = await res.json();
  const list = document.getElementById('taskList');
  list.innerHTML = '';
  for (const t of tasks) {
    const item = document.createElement('div');
    item.textContent = t.title;
    list.appendChild(item);
  }
}

async function addTask() {
  const title = prompt('Task title');
  if (!title) return;
  await fetch('/api/tasks', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({title})});
  await loadTasks();
}

document.addEventListener('DOMContentLoaded', loadTasks);
