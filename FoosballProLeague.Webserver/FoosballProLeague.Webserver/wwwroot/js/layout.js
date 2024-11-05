document.addEventListener('DOMContentLoaded', function () {
    const profileIcon = document.getElementById('profileIcon');
    const dropdownMenu = document.getElementById('dropdownMenu');
    const profileIconElement = profileIcon.querySelector('i');

    profileIcon.addEventListener('click', function (event) {
        event.stopPropagation();
        const isVisible = dropdownMenu.classList.contains('show');
        if (isVisible) {
            dropdownMenu.classList.remove('show');
            dropdownMenu.classList.add('hide');
            setTimeout(() => {
                dropdownMenu.classList.remove('hide');
                dropdownMenu.style.display = 'none';
            }, 300); // Match the duration of the fadeOut animation
        } else {
            dropdownMenu.style.display = 'block';
            dropdownMenu.classList.add('show');
        }
        profileIconElement.classList.toggle('active', !isVisible);
    });

    document.addEventListener('click', function () {
        if (dropdownMenu.classList.contains('show')) {
            dropdownMenu.classList.remove('show');
            dropdownMenu.classList.add('hide');
            setTimeout(() => {
                dropdownMenu.classList.remove('hide');
                dropdownMenu.style.display = 'none';
            }, 300); // Match the duration of the fadeOut animation
            profileIconElement.classList.remove('active');
        }
    });

    dropdownMenu.addEventListener('click', function (event) {
        event.stopPropagation();
    });
});
