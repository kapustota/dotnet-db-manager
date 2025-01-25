document.getElementById('articleForm').addEventListener('submit', async function(event) {
    event.preventDefault();

    const title = document.getElementById('title').value;
    const author = document.getElementById('author').value;
    const content = document.getElementById('content').value;
    const annotation = document.getElementById('annotation').value;
    const publishedDateInput = document.getElementById('publishedDate').value;

    const publishedDate = new Date(publishedDateInput).toISOString();

    const response = await fetch('/api/articles', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ title, author, content, annotation, published_date: publishedDate })
    });

    if (response.ok) {
        alert('Article added successfully');
    } else {
        alert('Failed to add article');
    }
});

// Обработчик формы поиска
document.getElementById('searchForm').addEventListener('submit', async function(event) {
    event.preventDefault();

    const searchTitle = document.getElementById('searchTitle').value;
    const searchAuthor = document.getElementById('searchAuthor').value;

    const queryParams = new URLSearchParams();
    if (searchTitle) queryParams.append('title', searchTitle);
    if (searchAuthor) queryParams.append('author', searchAuthor);

    const response = await fetch(`/api/articles/search?${queryParams.toString()}`);
    if (response.ok) {
        const articles = await response.json();
        displaySearchResults(articles);
    } else {
        alert('Failed to search articles');
    }
});

// Функция для отображения результатов поиска
function displaySearchResults(articles) {
    const resultsDiv = document.getElementById('searchResults');
    resultsDiv.innerHTML = '';

    if (articles.length === 0) {
        resultsDiv.innerHTML = '<p>No articles found.</p>';
        return;
    }

    articles.forEach(article => {
        const publishedDate = article.published_date ? new Date(article.published_date) : null;
        const formattedDate = publishedDate && !isNaN(publishedDate)
            ? publishedDate.toLocaleString()
            : 'Invalid Date';

        const articleDiv = document.createElement('div');
        articleDiv.className = 'article';
        articleDiv.innerHTML = `
            <p><strong>ID:</strong> ${article.id}</p>
            <h2>${article.title}</h2>
            <p><strong>Author:</strong> ${article.author}</p>
            <p>${article.content}</p>
            <p><strong>Annotation:</strong> ${article.annotation}</p>
            <p><strong>Published Date:</strong> ${formattedDate}</p>
            <button class="update-button" data-id="${article.id}">Update</button>
            <button class="delete-button" data-id="${article.id}">Delete</button>
        `;
        resultsDiv.appendChild(articleDiv);
    });

    // Добавить обработчики событий для кнопок Delete и Update
    attachButtonListeners();
}

// Функция для прикрепления обработчиков к кнопкам
function attachButtonListeners() {
    // Обработчик Delete
    const deleteButtons = document.querySelectorAll('.delete-button');
    deleteButtons.forEach(button => {
        button.addEventListener('click', async function() {
            const id = this.getAttribute('data-id');
            if (confirm('Are you sure you want to delete this article?')) {
                const response = await fetch(`/api/articles/${id}`, {
                    method: 'DELETE'
                });
                if (response.ok) {
                    alert('Article deleted successfully');
                    // Повторный поиск для обновления списка
                    document.getElementById('searchForm').dispatchEvent(new Event('submit'));
                } else {
                    alert('Failed to delete article');
                }
            }
        });
    });

    // Обработчик Update
    const updateButtons = document.querySelectorAll('.update-button');
    updateButtons.forEach(button => {
        button.addEventListener('click', async function() {
            const id = this.getAttribute('data-id');
            // Получение текущих данных статьи
            const response = await fetch(`/api/articles/${id}`);
            if (response.ok) {
                const article = await response.json();
                openUpdateModal(article);
            } else {
                alert('Failed to fetch article details');
            }
        });
    });
}

// Функция для открытия модального окна обновления
function openUpdateModal(article) {
    // Создание модального окна
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.innerHTML = `
        <div class="modal-content">
            <span class="close-button">&times;</span>
            <h2>Update Article</h2>
            <form id="updateForm">
                <input type="hidden" id="updateId" value="${article.id}">
                <input type="text" id="updateTitle" placeholder="Title" value="${article.title}" required>
                <input type="text" id="updateAuthor" placeholder="Author" value="${article.author}" required>
                <textarea id="updateContent" placeholder="Content" required>${article.content}</textarea>
                <textarea id="updateAnnotation" placeholder="Annotation">${article.annotation}</textarea> 
                <input type="datetime-local" id="updatePublishedDate" value="${formatDateForInput(article.published_date)}" required>
                <button type="submit">Save Changes</button>
            </form>
        </div>
    `;
    document.body.appendChild(modal);

    // Закрытие модального окна
    const closeButton = modal.querySelector('.close-button');
    closeButton.addEventListener('click', () => {
        modal.remove();
    });

    // Обработка формы обновления
    const updateForm = modal.querySelector('#updateForm');
    updateForm.addEventListener('submit', async function(event) {
        event.preventDefault();

        const id = document.getElementById('updateId').value;
        const title = document.getElementById('updateTitle').value;
        const author = document.getElementById('updateAuthor').value;
        const content = document.getElementById('updateContent').value;
        const annotation = document.getElementById('updateAnnotation').value;
        const publishedDateInput = document.getElementById('updatePublishedDate').value;

        const publishedDate = new Date(publishedDateInput).toISOString();

        const response = await fetch(`/api/articles/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ id: parseInt(id), title, author, content, annotation, published_date: publishedDate })
        });

        if (response.ok) {
            alert('Article updated successfully');
            modal.remove();
            // Повторный поиск для обновления списка
            document.getElementById('searchForm').dispatchEvent(new Event('submit'));
        } else {
            alert('Failed to update article');
        }
    });
}

// Вспомогательная функция для форматирования даты для input[type="datetime-local"]
function formatDateForInput(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    const tzOffset = date.getTimezoneOffset() * 60000; // offset in milliseconds
    const localISOTime = new Date(date - tzOffset).toISOString().slice(0, -1);
    return localISOTime.substring(0, 16); // формат YYYY-MM-DDTHH:mm
}

document.getElementById('exportPdfBtn').addEventListener('click', async () => {
    const response = await fetch('/api/articles/pdf');
    if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'articles.pdf';
        link.click();
        URL.revokeObjectURL(url);
    } else {
        alert('Failed to get PDF');
    }
});