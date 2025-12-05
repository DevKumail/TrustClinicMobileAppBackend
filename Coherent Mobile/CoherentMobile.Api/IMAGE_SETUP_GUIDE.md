# 📁 **Image Setup Guide for Clinic Information**

## Overview
This guide explains how to set up the image directory structure for the clinic information system.

---

## 🗂️ **Folder Structure**

Create the following folder structure in your API project:

```
CoherentMobile.Api/
└── wwwroot/
    └── images/
        ├── doctors/          (Doctor profile images)
        ├── facilities/       (Clinic/facility images)
        ├── services/         (Service display images)
        └── icons/            (Service icon images)
```

---

## ⚙️ **Setup Instructions**

### **Step 1: Create Directories**

Run these commands from the API project directory:

```bash
# Create main images directory
mkdir -p wwwroot/images/doctors
mkdir -p wwwroot/images/facilities
mkdir -p wwwroot/images/services
mkdir -p wwwroot/images/icons
```

**Or on Windows:**
```powershell
New-Item -ItemType Directory -Path "wwwroot\images\doctors" -Force
New-Item -ItemType Directory -Path "wwwroot\images\facilities" -Force
New-Item -ItemType Directory -Path "wwwroot\images\services" -Force
New-Item -ItemType Directory -Path "wwwroot\images\icons" -Force
```

---

### **Step 2: Add Sample Images**

Place your images in the appropriate folders:

#### **Doctor Images** (`wwwroot/images/doctors/`)
- `dr_walid.jpg` - Dr. Walid Reda Sayed profile picture
- `dr_muna.jpg` - Dr. Muna Amam profile picture
- `dr_nayrouz.jpg` - Dr. Nayrouz Gezaf profile picture

#### **Facility Images** (`wwwroot/images/facilities/`)
- `clinic_image_1.jpg` - Main clinic image
- `clinic_image_2.jpg` - Reception area
- `clinic_image_3.jpg` - Treatment room

#### **Service Images** (`wwwroot/images/services/`)
- `service1.jpg` - Fertility Assessment service image
- `service2.jpg` - Assisted Reproductive service image

#### **Service Icons** (`wwwroot/images/icons/`)
- `fertility_assessment_icon.png` - Fertility Assessment icon
- `assisted_reproductive_icon.png` - Assisted Reproductive icon

---

## 🔗 **Image URLs**

After setup, images will be accessible at:

```
https://localhost:7162/images/doctors/dr_walid.jpg
https://localhost:7162/images/facilities/clinic_image_1.jpg
https://localhost:7162/images/services/service1.jpg
https://localhost:7162/images/icons/fertility_assessment_icon.png
```

---

## ⚙️ **Configuration**

Image paths are configured in `appsettings.json`:

```json
{
  "ImageSettings": {
    "VirtualPath": "/images",
    "PhysicalPath": "wwwroot/images",
    "BaseUrl": "https://localhost:7162/images",
    "DoctorImagesPath": "doctors",
    "FacilityImagesPath": "facilities",
    "ServiceImagesPath": "services",
    "ServiceIconsPath": "icons",
    "MaxFileSizeInMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"]
  }
}
```

---

## 📝 **Database Image References**

Images are referenced in the database by filename only (not full path):

### **MDoctors Table**
```sql
UPDATE MDoctors 
SET DoctorPhotoName = 'dr_walid.jpg' 
WHERE DId = 1;
```

### **MFacility Table**
```sql
UPDATE MFacility 
SET FacilityImages = 'clinic_image_1.jpg,clinic_image_2.jpg,clinic_image_3.jpg' 
WHERE FId = 1;
```

### **MServices Table**
```sql
UPDATE MServices 
SET DisplayImageName = 'service1.jpg',
    IconImageName = 'fertility_assessment_icon.png'
WHERE SId = 1;
```

---

## 🎨 **Image Requirements**

### **Doctor Images**
- Format: JPG, PNG
- Recommended Size: 400x400px (square)
- Max File Size: 2MB
- Background: Professional, neutral

### **Facility Images**
- Format: JPG, PNG, WebP
- Recommended Size: 1200x800px (landscape)
- Max File Size: 5MB
- Quality: High-resolution

### **Service Images**
- Format: JPG, PNG, WebP
- Recommended Size: 800x600px
- Max File Size: 3MB

### **Service Icons**
- Format: PNG (with transparency)
- Recommended Size: 128x128px or 256x256px (square)
- Max File Size: 500KB

---

## 🔒 **Security Notes**

1. **File Type Validation**: Only allowed extensions can be uploaded
2. **File Size Limits**: Configured in `ImageSettings:MaxFileSizeInMB`
3. **Static File Access**: Images are publicly accessible (no authentication required)
4. **Path Traversal Protection**: ASP.NET Core handles this automatically

---

## 🧪 **Testing Image Access**

Test if images are accessible:

```bash
# Test doctor image
curl https://localhost:7162/images/doctors/dr_walid.jpg

# Test facility image
curl https://localhost:7162/images/facilities/clinic_image_1.jpg

# Test service icon
curl https://localhost:7162/images/icons/fertility_assessment_icon.png
```

Or open in browser:
- https://localhost:7162/images/doctors/dr_walid.jpg
- https://localhost:7162/images/facilities/clinic_image_1.jpg

---

## 🚀 **Production Deployment**

For production, consider:

1. **CDN**: Use a CDN for faster image delivery
2. **Cloud Storage**: Azure Blob Storage, AWS S3, etc.
3. **Image Optimization**: Compress images before upload
4. **Caching**: Configure proper cache headers
5. **Backup**: Regular backups of image files

### **Example CDN Configuration**

Update `appsettings.Production.json`:

```json
{
  "ImageSettings": {
    "BaseUrl": "https://cdn.trustfertility.ae/images",
    "VirtualPath": "/images",
    "PhysicalPath": "wwwroot/images"
  }
}
```

---

## 📊 **Current Database Image Mapping**

| Table | Column | Example Value | Full URL |
|-------|--------|---------------|----------|
| MDoctors | DoctorPhotoName | `dr_walid.jpg` | `{BaseUrl}/doctors/dr_walid.jpg` |
| MFacility | FacilityImages | `clinic_image_1.jpg,clinic_image_2.jpg` | `{BaseUrl}/facilities/clinic_image_1.jpg` |
| MServices | DisplayImageName | `service1.jpg` | `{BaseUrl}/services/service1.jpg` |
| MServices | IconImageName | `fertility_assessment_icon.png` | `{BaseUrl}/icons/fertility_assessment_icon.png` |

---

## ✅ **Verification Checklist**

- [ ] Created `wwwroot/images` directory structure
- [ ] Added sample images to appropriate folders
- [ ] Updated database with correct image filenames
- [ ] Tested image URLs in browser
- [ ] Configured `ImageSettings` in appsettings.json
- [ ] Verified API returns correct image URLs
- [ ] Tested guest mode endpoint `/api/guest/clinic-info`

---

## 🆘 **Troubleshooting**

### **Images not loading?**

1. **Check folder permissions**: Ensure IIS/Kestrel has read access
2. **Verify file names match database**: Case-sensitive on Linux
3. **Check appsettings.json**: Confirm `BaseUrl` is correct
4. **Test static files middleware**: Ensure `app.UseStaticFiles()` is called
5. **Clear browser cache**: Hard refresh (Ctrl+F5)

### **404 Error on image URL?**

- Verify file exists in correct folder
- Check filename spelling (case-sensitive)
- Ensure `UseStaticFiles()` is before `UseAuthorization()`
- Check IIS virtual directory configuration

---

**Setup complete! Images ab dynamically serve ho rahe hain!** 📸✨
