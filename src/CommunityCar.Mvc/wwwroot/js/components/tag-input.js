/**
 * Tag Input Component with Autocomplete
 * Allows users to add up to 3 tags with suggestions
 */

class TagInput {
    constructor(inputElement, options = {}) {
        this.input = inputElement;
        this.maxTags = options.maxTags || 3;
        this.suggestions = options.suggestions || this.getDefaultSuggestions();
        this.tags = [];
        
        this.init();
    }
    
    getDefaultSuggestions() {
        return [
            'maintenance', 'repair', 'modification', 'review', 'tips',
            'engine', 'transmission', 'brakes', 'suspension', 'electrical',
            'interior', 'exterior', 'performance', 'fuel', 'oil',
            'tires', 'wheels', 'battery', 'cooling', 'heating',
            'diy', 'tutorial', 'guide', 'problem', 'solution',
            'upgrade', 'install', 'replace', 'fix', 'clean',
            'sedan', 'suv', 'truck', 'sports', 'luxury',
            'budget', 'expensive', 'cheap', 'quality', 'safety'
        ];
    }
    
    init() {
        // Create container
        this.container = document.createElement('div');
        this.container.className = 'tag-input-container';
        
        // Create tags display area
        this.tagsDisplay = document.createElement('div');
        this.tagsDisplay.className = 'tags-display';
        
        // Create input wrapper
        this.inputWrapper = document.createElement('div');
        this.inputWrapper.className = 'tag-input-wrapper';
        
        // Create new input for typing
        this.typeInput = document.createElement('input');
        this.typeInput.type = 'text';
        this.typeInput.className = 'tag-type-input';
        this.typeInput.placeholder = 'Type to add tags (max 3)...';
        this.typeInput.autocomplete = 'off';
        
        // Create suggestions dropdown
        this.suggestionsDropdown = document.createElement('div');
        this.suggestionsDropdown.className = 'tag-suggestions';
        this.suggestionsDropdown.style.display = 'none';
        
        // Hide original input
        this.input.style.display = 'none';
        
        // Build structure
        this.inputWrapper.appendChild(this.tagsDisplay);
        this.inputWrapper.appendChild(this.typeInput);
        this.container.appendChild(this.inputWrapper);
        this.container.appendChild(this.suggestionsDropdown);
        
        // Insert after original input
        this.input.parentNode.insertBefore(this.container, this.input.nextSibling);
        
        // Load existing tags
        this.loadExistingTags();
        
        // Bind events
        this.bindEvents();
    }
    
    loadExistingTags() {
        const existingValue = this.input.value;
        if (existingValue) {
            const tags = existingValue.split(',').map(t => t.trim()).filter(t => t);
            tags.slice(0, this.maxTags).forEach(tag => this.addTag(tag, false));
        }
    }
    
    bindEvents() {
        // Type input events
        this.typeInput.addEventListener('input', (e) => this.handleInput(e));
        this.typeInput.addEventListener('keydown', (e) => this.handleKeydown(e));
        this.typeInput.addEventListener('focus', () => this.showSuggestions());
        this.typeInput.addEventListener('blur', () => {
            setTimeout(() => this.hideSuggestions(), 200);
        });
        
        // Click outside to close suggestions
        document.addEventListener('click', (e) => {
            if (!this.container.contains(e.target)) {
                this.hideSuggestions();
            }
        });
    }
    
    handleInput(e) {
        const value = e.target.value.trim();
        
        if (value.length > 0) {
            this.filterSuggestions(value);
        } else {
            this.showAllSuggestions();
        }
    }
    
    handleKeydown(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const value = this.typeInput.value.trim();
            if (value) {
                this.addTag(value);
                this.typeInput.value = '';
                this.hideSuggestions();
            }
        } else if (e.key === 'Backspace' && this.typeInput.value === '' && this.tags.length > 0) {
            // Remove last tag on backspace if input is empty
            this.removeTag(this.tags[this.tags.length - 1]);
        }
    }
    
    addTag(tagText, updateInput = true) {
        // Normalize tag
        tagText = tagText.toLowerCase().trim();
        
        // Check if already exists
        if (this.tags.includes(tagText)) {
            this.showMessage('Tag already added', 'warning');
            return;
        }
        
        // Check max tags
        if (this.tags.length >= this.maxTags) {
            this.showMessage(`Maximum ${this.maxTags} tags allowed`, 'error');
            return;
        }
        
        // Add tag
        this.tags.push(tagText);
        
        // Create tag element
        const tagElement = document.createElement('span');
        tagElement.className = 'tag-badge';
        tagElement.innerHTML = `
            ${tagText}
            <button type="button" class="tag-remove" data-tag="${tagText}">
                <i class="fas fa-times"></i>
            </button>
        `;
        
        // Add remove handler
        tagElement.querySelector('.tag-remove').addEventListener('click', () => {
            this.removeTag(tagText);
        });
        
        this.tagsDisplay.appendChild(tagElement);
        
        // Update hidden input
        if (updateInput) {
            this.updateHiddenInput();
        }
        
        // Update placeholder
        this.updatePlaceholder();
    }
    
    removeTag(tagText) {
        const index = this.tags.indexOf(tagText);
        if (index > -1) {
            this.tags.splice(index, 1);
            
            // Remove from DOM
            const tagElements = this.tagsDisplay.querySelectorAll('.tag-badge');
            tagElements.forEach(el => {
                if (el.textContent.trim().startsWith(tagText)) {
                    el.remove();
                }
            });
            
            // Update hidden input
            this.updateHiddenInput();
            
            // Update placeholder
            this.updatePlaceholder();
        }
    }
    
    updateHiddenInput() {
        this.input.value = this.tags.join(', ');
    }
    
    updatePlaceholder() {
        if (this.tags.length >= this.maxTags) {
            this.typeInput.placeholder = 'Maximum tags reached';
            this.typeInput.disabled = true;
        } else {
            this.typeInput.placeholder = `Type to add tags (${this.tags.length}/${this.maxTags})...`;
            this.typeInput.disabled = false;
        }
    }
    
    filterSuggestions(query) {
        const filtered = this.suggestions.filter(tag => 
            tag.toLowerCase().includes(query.toLowerCase()) && 
            !this.tags.includes(tag)
        );
        
        this.renderSuggestions(filtered.slice(0, 10));
    }
    
    showAllSuggestions() {
        const available = this.suggestions.filter(tag => !this.tags.includes(tag));
        this.renderSuggestions(available.slice(0, 10));
    }
    
    renderSuggestions(suggestions) {
        this.suggestionsDropdown.innerHTML = '';
        
        if (suggestions.length === 0) {
            this.suggestionsDropdown.style.display = 'none';
            return;
        }
        
        suggestions.forEach(tag => {
            const item = document.createElement('div');
            item.className = 'tag-suggestion-item';
            item.textContent = tag;
            item.addEventListener('click', () => {
                this.addTag(tag);
                this.typeInput.value = '';
                this.typeInput.focus();
            });
            this.suggestionsDropdown.appendChild(item);
        });
        
        this.suggestionsDropdown.style.display = 'block';
    }
    
    showSuggestions() {
        if (this.tags.length < this.maxTags) {
            const value = this.typeInput.value.trim();
            if (value) {
                this.filterSuggestions(value);
            } else {
                this.showAllSuggestions();
            }
        }
    }
    
    hideSuggestions() {
        this.suggestionsDropdown.style.display = 'none';
    }
    
    showMessage(message, type) {
        if (window.Toast) {
            window.Toast.show(message, type);
        } else {
            console.log(`[${type}] ${message}`);
        }
    }
}

// Initialize tag inputs on page load
document.addEventListener('DOMContentLoaded', function() {
    const tagInputs = document.querySelectorAll('input[data-tag-input="true"]');
    tagInputs.forEach(input => {
        new TagInput(input, {
            maxTags: parseInt(input.dataset.maxTags) || 3,
            suggestions: input.dataset.suggestions ? JSON.parse(input.dataset.suggestions) : undefined
        });
    });
});

// Export for manual initialization
window.TagInput = TagInput;
