function collapseFilterGroup(event) {
    const svg = event.target.closest('svg');
    const currentTransform = svg.style.transform;
    const filterComponents = document.getElementsByClassName('filter-components');

    if (currentTransform.includes('scaleY(-1)')) {
        svg.style.transform = 'scaleY(1)';
        for (let i = 0; i < filterComponents.length; i++) {
            filterComponents[i].classList.remove('collapsed');
        }
    } else {
        svg.style.transform = 'scaleY(-1)';
        for (let i = 0; i < filterComponents.length; i++) {
            filterComponents[i].classList.add('collapsed');
        }
    }
}